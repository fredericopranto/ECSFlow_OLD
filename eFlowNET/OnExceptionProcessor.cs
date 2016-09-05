using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ECSFlow.Fody
{
    /// <summary>
    /// Main exception processor
    /// </summary>
    public class OnExceptionProcessor
    {
        public MethodDefinition Method;
        public ModuleWeaver ModuleWeaver;
        public MethodBody Body;

        MethodFinder MethodFinder;
        AttributeFinder AttributeFinder;
        ExceptionDefinitionFinder ExceptionFinder;

        /// <summary>
        /// Process the current method in case of match the configuration in ECSFlowExceptionDefinitionFile
        /// </summary>
        public void Process()
        {
            //If has no customattribute, not process
            if (Method.Resolve().CustomAttributes.Count > 0)
            {
                if (Method.Name.Contains("LoadImage"))
                {
                    ExceptionFinder = new ExceptionDefinitionFinder(Method);
                    if (!ExceptionFinder.Inpect)
                    {
                        return;
                    }

                    ContinueProcessing(ExceptionFinder);
                }
            }
        }

        /// <summary>
        /// In case of having Raising Site, the current method is surrounded by a Try/Catch/Finally block
        /// </summary>
        /// <param name="exceptionFinder"></param>
        void ContinueProcessing(ExceptionDefinitionFinder exceptionFinder)
        {
            InjectAttributes(exceptionFinder);

            AttributeFinder = new AttributeFinder(Method);
            if (AttributeFinder.Raising)
            {
                SurroundBody(AttributeFinder);
            }
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
            }
        }

        /// <summary>
        /// //todo: add finally block too
        ///  Surround current Method Body with Try/Catch
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

            var tryBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);
            var catchBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);

            var methodBodyFirstInstruction = GetMethodBodyFirstInstruction();

            if (attributeFinder.Exceptions.Count == 0)
                attributeFinder.Exceptions.Add(ModuleWeaver.ExceptionType);

            foreach (var exceptionType in attributeFinder.Exceptions)
            {
                // Find the proper handler by exception type
                MethodFinder = new MethodFinder(exceptionType);
                
                if (MethodFinder.Found) // Surround with Try/Catch and Inject the proper handler
                {
                    var writeLineRef = Body.Method.Module.Assembly.MainModule.ImportReference(exceptionType); 

                    var catchBlockInstructions = GetCatchInstructions(catchBlockLeaveInstructions, MethodFinder.Method).ToList();
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
                else // Surround with Try/Catch and Inject the throws handler
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
        IEnumerable<Instruction> GetCatchInstructions(Instruction catchBlockLeaveInstructions, MethodDefinition def)
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
