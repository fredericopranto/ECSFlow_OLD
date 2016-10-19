using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using ECSFlow.Finder;

namespace ECSFlow
{

    public static class Extensions
    {
        public static Instruction CreateLoadInstruction(this ILProcessor self, object obj)
        {
            if (obj is string)
                return self.Create(OpCodes.Ldstr, obj as string);
            else if (obj is int)
                return self.Create(OpCodes.Ldc_I4, (int)obj);

            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Main exception processor
    /// </summary>
    public class OnExceptionProcessor
    {
        public string AssemblyPath;
        public AssemblyDefinition AssemblyDefinition;

        public MethodDefinition Method;
        public ModuleWeaver ModuleWeaver;
        public MethodBody Body;

        MethodFinder MethodFinder;
        AttributeFinder AttributeFinder;
        ExceptionDefinitionFinder ExceptionFinder;

        private const string _preMethodName = "PreMethod";
        private const string _postMethodName = "PostMethod";
        private const string _exceptionMethodName = "ExceptionMethod";

        private const string _procMethodName = "Process";

        private const string _getMethodName = "Get";
        private const string _setMethodName = "Set";

        /// <summary>
        /// Process the current method in case of match the configuration in ECSFlowExceptionDefinitionFile
        /// </summary>
        public void Process()
        {
            //If has no customattribute, not process
            if (Method.Resolve().CustomAttributes.Count > 0)
            {}

            ExceptionFinder = new ExceptionDefinitionFinder(Method);
            if (!ExceptionFinder.Inpect)
            {
                return;
            }

            ContinueProcessing(ExceptionFinder);
        }

        /// <summary>
        /// In case of having Raising Site, the current method is surrounded by a Try/Catch/Finally block
        /// </summary>
        /// <param name="exceptionFinder"></param>
        void ContinueProcessing(ExceptionDefinitionFinder exceptionFinder)
        {
            //InjectAttributes(exceptionFinder);

            AttributeFinder = new AttributeFinder(Method);
            if (AttributeFinder.Raising)
            {
                SurroundBody(AttributeFinder);
            }
        }

        public void RewriteMethods()
        {
            foreach (var module in AssemblyDefinition.Modules)
            {
                foreach (var type in module.Types)
                {
                    foreach (var currentMethod in type.Methods)
                    {
                        var ilProcessor = currentMethod.Body.GetILProcessor();
                        var firstUserInstruction = ilProcessor.Body.Instructions.First();

                        foreach (var att in currentMethod.CustomAttributes)
                        {
                            ((BaseAssemblyResolver)((MetadataResolver)att.AttributeType.Module.MetadataResolver).AssemblyResolver).AddSearchDirectory(System.IO.Path.GetDirectoryName(AssemblyPath));
                        }


                        int currentAttribute = 0;
                        foreach (var att in currentMethod.CustomAttributes)
                        {
                            currentMethod.Body.InitLocals = true;
                            currentMethod.Body.Variables.Add(new VariableDefinition(att.AttributeType));

                            ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.Create(OpCodes.Newobj, currentMethod.Module.ImportReference(att.Constructor)));

                            ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.Create(OpCodes.Stloc, currentAttribute));
                            currentAttribute++;
                        }
                        currentAttribute = 0;
                        foreach (var att in currentMethod.CustomAttributes)
                        {
                            var preMethod = att.AttributeType.Resolve().Methods.FirstOrDefault(x => x.Name == _preMethodName);
                            if (preMethod != null)
                            {
                                ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.Create(OpCodes.Ldloc, currentAttribute));
                                currentAttribute++;
                                AddInterceptCall(ilProcessor, currentMethod, preMethod, att, firstUserInstruction);
                            }
                        }

                        int currentParameter = 0;
                        foreach (var para in currentMethod.Parameters)
                        {
                            foreach (var att in para.CustomAttributes)
                            {
                                ((BaseAssemblyResolver)((MetadataResolver)att.AttributeType.Module.MetadataResolver).AssemblyResolver).AddSearchDirectory(System.IO.Path.GetDirectoryName(AssemblyPath));

                                var processMeth = att.AttributeType.Resolve().Methods.FirstOrDefault(x => x.Name == _procMethodName);
                                if (processMeth != null)
                                {
                                    ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.CreateLoadInstruction(currentMethod.Name));
                                    ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.CreateLoadInstruction(para.Name));

                                    ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.Create(OpCodes.Ldarga_S, para));

                                    ilProcessor.InsertBefore(firstUserInstruction, ilProcessor.Create(OpCodes.Call, currentMethod.Module.ImportReference(processMeth)));
                                }
                            }
                            currentParameter++;
                        }
                        if (currentMethod.HasCustomAttributes)
                        {

                            var returnInstruction = NormalizeReturns(currentMethod);

                            var tryStart = currentMethod.Body.Instructions[2 * currentMethod.CustomAttributes.Count];
                            var beforeReturnInstruction = Instruction.Create(OpCodes.Nop);

                            ilProcessor.InsertBefore(returnInstruction, beforeReturnInstruction);

                            var afterPostInstruction = Instruction.Create(OpCodes.Nop);
                            ilProcessor.InsertBefore(returnInstruction, afterPostInstruction);

                            var beforePostInstruction = Instruction.Create(OpCodes.Nop);
                            ilProcessor.InsertBefore(afterPostInstruction, beforePostInstruction);

                            currentAttribute = 0;
                            foreach (var att in currentMethod.CustomAttributes)
                            {
                                var postMethod = att.AttributeType.Resolve().Methods.FirstOrDefault(x => x.Name == _postMethodName);
                                if (postMethod != null)
                                {
                                    ilProcessor.InsertBefore(afterPostInstruction, ilProcessor.Create(OpCodes.Ldloc, currentAttribute));
                                    currentAttribute++;
                                    AddInterceptCall(ilProcessor, currentMethod, postMethod, att, afterPostInstruction);
                                }
                            }

                            ilProcessor.InsertBefore(returnInstruction, Instruction.Create(OpCodes.Endfinally));

                            var finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
                            {
                                TryStart = tryStart,
                                TryEnd = beforePostInstruction,
                                HandlerStart = beforePostInstruction,
                                HandlerEnd = returnInstruction,
                            };

                            currentMethod.Body.ExceptionHandlers.Add(finallyHandler);
                            currentMethod.Body.InitLocals = true;
                        }
                    }
                }
            }
        }

        Instruction NormalizeReturns(MethodDefinition Method)
        {
            if (Method.ReturnType == Method.Module.TypeSystem.Void)
            {
                var instructions = Method.Body.Instructions;
                var lastRet = Instruction.Create(OpCodes.Ret);
                instructions.Add(lastRet);

                for (var index = 0; index < Method.Body.Instructions.Count - 1; index++)
                {
                    var instruction = instructions[index];
                    if (instruction.OpCode == OpCodes.Ret)
                    {
                        instructions[index] = Instruction.Create(OpCodes.Leave, lastRet);
                    }
                }
                return lastRet;
            }
            else
            {
                var instructions = Method.Body.Instructions;
                var returnVariable = new VariableDefinition("methodTimerReturn", Method.ReturnType);
                Method.Body.Variables.Add(returnVariable);
                var lastLd = Instruction.Create(OpCodes.Ldloc, returnVariable);
                instructions.Add(lastLd);
                instructions.Add(Instruction.Create(OpCodes.Ret));

                for (var index = 0; index < instructions.Count - 2; index++)
                {
                    var instruction = instructions[index];
                    if (instruction.OpCode == OpCodes.Ret)
                    {
                        instructions[index] = Instruction.Create(OpCodes.Leave, lastLd);
                        instructions.Insert(index, Instruction.Create(OpCodes.Stloc, returnVariable));
                        index++;
                    }
                }
                return lastLd;
            }
        }

        private void CreateAttrObjectInMethod(ILProcessor ilProcessor, Instruction firstInstruction, MethodDefinition methDef, CustomAttribute att)
        {
            methDef.Body.InitLocals = true;

            methDef.Body.Variables.Add(new VariableDefinition(att.AttributeType));

            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Newobj, methDef.Module.ImportReference(att.Constructor)));

        }

        private Instruction AddInterceptCall(ILProcessor ilProcessor, MethodDefinition methDef, MethodDefinition interceptMethDef, CustomAttribute att, Instruction insertBefore)
        {
            var methRef = AssemblyDefinition.MainModule.ImportReference(interceptMethDef);

            ilProcessor.InsertBefore(insertBefore, ilProcessor.CreateLoadInstruction(methDef.Name));

            int methodParamCount = methDef.Parameters.Count;
            int arrayVarNr = methDef.Body.Variables.Count;

            if (methodParamCount > 0)
            {
                ArrayType objArrType = new ArrayType(AssemblyDefinition.MainModule.TypeSystem.Object);
                methDef.Body.Variables.Add(new VariableDefinition(objArrType));

                methDef.Body.InitLocals = true;

                ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldc_I4, methodParamCount));
                ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Newarr, AssemblyDefinition.MainModule.TypeSystem.Object));
                ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Stloc, arrayVarNr));

                bool pointerToValueTypeVariable;
                TypeSpecification referencedTypeSpec = null;

                for (int i = 0; i < methodParamCount; i++)
                {
                    var paramMetaData = methDef.Parameters[i].ParameterType.MetadataType;
                    if (paramMetaData == MetadataType.UIntPtr || paramMetaData == MetadataType.FunctionPointer ||
                        paramMetaData == MetadataType.IntPtr || paramMetaData == MetadataType.Pointer)
                    {
                        break;
                    }

                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldloc, arrayVarNr));
                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldc_I4, i));

                    if (methDef.IsStatic)
                    {
                        ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldarg, i));
                    }
                    else
                    {
                        ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldarg, i + 1));
                    }

                    pointerToValueTypeVariable = false;

                    TypeReference paramType = methDef.Parameters[i].ParameterType;
                    if (paramType.IsByReference)
                    {
                        referencedTypeSpec = paramType as TypeSpecification;

                        if (referencedTypeSpec != null)
                        {
                            switch (referencedTypeSpec.ElementType.MetadataType)
                            {
                                case MetadataType.Boolean:
                                case MetadataType.SByte:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_I1));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.Int16:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_I2));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.Int32:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_I4));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.Int64:
                                case MetadataType.UInt64:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_I8));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.Byte:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_U1));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.UInt16:
                                case MetadataType.Char:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_U2));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.UInt32:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_U4));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.Single:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_R4));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.Double:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_R8));
                                    pointerToValueTypeVariable = true;
                                    break;

                                case MetadataType.IntPtr:
                                case MetadataType.UIntPtr:
                                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_I));
                                    pointerToValueTypeVariable = true;
                                    break;

                                default:
                                    if (referencedTypeSpec.ElementType.IsValueType)
                                    {
                                        ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldobj, referencedTypeSpec.ElementType));
                                        pointerToValueTypeVariable = true;
                                    }
                                    else
                                    {
                                        ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldind_Ref));
                                        pointerToValueTypeVariable = false;
                                    }
                                    break;
                            }
                        }
                        else
                        {

                        }
                    }

                    if (paramType.IsValueType || pointerToValueTypeVariable)
                    {
                        if (pointerToValueTypeVariable)
                        {
                            ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Box, referencedTypeSpec.ElementType));
                        }
                        else
                        {
                            ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Box, paramType));
                        }
                    }
                    ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Stelem_Ref));
                }


                ilProcessor.InsertBefore(insertBefore, ilProcessor.Create(OpCodes.Ldloc, arrayVarNr));
            }
            var ins = ilProcessor.Create(OpCodes.Callvirt, methDef.Module.ImportReference(interceptMethDef));
            ilProcessor.InsertBefore(insertBefore, ins);
            return ins;
        }

        /// <summary>
        /// Inject custom attributes in current method
        /// </summary>
        /// <param name="exceptionFinder"></param>
        void InjectAttributes(ExceptionDefinitionFinder exceptionFinder)
        {
            var customAttributes = exceptionFinder.CustomAttributes;
            foreach (CustomAttribute item in customAttributes)
            {
                Method.CustomAttributes.Add(item);
                Method.Module.ImportReference(item.Constructor);
            }
        }

        /// <summary>
        ///  Surround current Method Body with Try/Catch
        ///  If has a mapping of a proper handler add the custom method in catch block
        ///  If dosen't, add a default method to show a message (for tests only)
        /// </summary>
        void SurroundBody(AttributeFinder attributeFinder)
        {
            Body = Method.Body;
            Body.SimplifyMacros();

            var ilProcessor = Body.GetILProcessor();

            var returnFixer = new ReturnFixer
            {
                Method = Method
            };
            returnFixer.MakeLastStatementReturn();

            // Create a basic Try/Cacth Block
            var tryBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);
            var catchBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);

            // Get the first instruction to surround the Try/Catch Block
            var methodBodyFirstInstruction = GetMethodBodyFirstInstruction();

            // Get the list of Exception Types guarded by the Explicit Channel
            if (attributeFinder.Exceptions.Count == 0)
                attributeFinder.Exceptions.Add(ModuleWeaver.ExceptionType);

            // Get mapping reference
            ModuleDefinition ECSFlowModule = ModuleDefinition.ReadModule("ECSFlowAttributes.dll");
            TypeDefinition MappingType = ECSFlowModule.Types.First(t => t.FullName == "AssemblyToProcessMapping");
            
            // Create a Catch Block for each Exception Type
            foreach (var exceptionType in attributeFinder.Exceptions)
            {
                // Find the proper handler by exception type
                MethodFinder = new MethodFinder(exceptionType, MappingType);

                if (MethodFinder.Found) // Surround with Try/Catch and Inject the proper handler
                {
                    var methodRef = Body.Method.Module.ImportReference(MethodFinder.MethodReference);

                    var catchBlockInstructions = GetCatchInstructions(catchBlockLeaveInstructions, methodRef).ToList();
                    ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, tryBlockLeaveInstructions);
                    ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, catchBlockInstructions);

                    var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                    {
                        CatchType = exceptionType,
                        TryStart = methodBodyFirstInstruction,
                        TryEnd = tryBlockLeaveInstructions.Next,
                        HandlerStart = catchBlockInstructions.First(),
                        HandlerEnd = catchBlockInstructions.Last().Next
                    };

                    Body.ExceptionHandlers.Add(handler);
                }
                else // Surround with Try/Catch and Inject the default handler
                {
                    //TODO: Inject throws statement
                    var catchBlockInstructions = GetCatchInstructions(catchBlockLeaveInstructions).ToList();
                    ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, tryBlockLeaveInstructions);
                    ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, catchBlockInstructions);


                    var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                    {
                        CatchType = exceptionType,
                        TryStart = methodBodyFirstInstruction,
                        TryEnd = tryBlockLeaveInstructions.Next,
                        HandlerStart = catchBlockInstructions.First(),
                        HandlerEnd = catchBlockInstructions.Last().Next
                    };

                    Body.ExceptionHandlers.Add(handler);
                }
            }

            Body.InitLocals = true;
            Body.OptimizeMacros();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Instruction GetMethodBodyFirstInstruction()
        {
            if (Method.IsConstructor)
            {
                return Body.Instructions.First(i => i.OpCode == OpCodes.Call).Next;
            }
            return Body.Instructions.First();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="catchBlockLeaveInstructions"></param>
        /// <returns></returns>
        IEnumerable<Instruction> GetCatchInstructions(Instruction catchBlockLeaveInstructions, MethodReference def)
        {
            yield return Instruction.Create(OpCodes.Call, def);
            yield return Instruction.Create(OpCodes.Nop);
            yield return catchBlockLeaveInstructions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="catchBlockLeaveInstructions"></param>
        /// <returns></returns>
        IEnumerable<Instruction> GetCatchInstructions(Instruction catchBlockLeaveInstructions)
        {
            yield return Instruction.Create(OpCodes.Pop);
            yield return Instruction.Create(OpCodes.Nop);
            yield return catchBlockLeaveInstructions;
        }
    }
}
