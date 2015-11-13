using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;

namespace DotNetFlow.Fody
{
    public class OnExceptionProcessor
    {
        public MethodDefinition Method;
        public ModuleWeaver ModuleWeaver;
        MethodBody body;

        AttributeFinder attributeFinder;
        ExceptionDefinitionFinder exceptionFinder;

        public void Process()
        {
            exceptionFinder = new ExceptionDefinitionFinder(Method);
            if (!exceptionFinder.Inpect)
            {
                return;
            }

            ContinueProcessing(exceptionFinder);
        }

        void ContinueProcessing(ExceptionDefinitionFinder exceptionFinder)
        {
            //InjectAttributes(exceptionFinder);

            attributeFinder = new AttributeFinder(Method);
            if (attributeFinder.Raising)
            {
                SurroundBody();
            }
        }

        void InjectAttributes(ExceptionDefinitionFinder exceptionFinder)
        {
            var customAttributes = exceptionFinder.CustomAttributes;
            foreach (CustomAttribute item in customAttributes)
            {
                Method.Module.ImportReference(item.GetType());
                Method.Module.ImportReference(typeof(System.Runtime.CompilerServices.ExtensionAttribute));
                Method.CustomAttributes.Add(item);
            }
        }

        /// <summary>
        ///  Surround Method Body with Try/Catch
        /// </summary>
        void SurroundBody()
        {
            body = Method.Body;

            body.SimplifyMacros();

            var ilProcessor = body.GetILProcessor();

            var returnFixer = new ReturnFixer
            {
                Method = Method
            };
            returnFixer.MakeLastStatementReturn();

            var tryBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);
            var catchBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);

            var methodBodyFirstInstruction = GetMethodBodyFirstInstruction();

            var catchBlockInstructions = GetCatchInstructions(catchBlockLeaveInstructions).ToList();

            ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, tryBlockLeaveInstructions);

            ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, catchBlockInstructions);

            var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = ModuleWeaver.ExceptionType,
                TryStart = methodBodyFirstInstruction,
                TryEnd = tryBlockLeaveInstructions.Next,
                HandlerStart = catchBlockInstructions.First(),
                HandlerEnd = catchBlockInstructions.Last().Next
            };

            body.ExceptionHandlers.Add(handler);

            body.InitLocals = true;
            body.OptimizeMacros();
        }

        Instruction GetMethodBodyFirstInstruction()
        {
            if (Method.IsConstructor)
            {
                return body.Instructions.First(i => i.OpCode == OpCodes.Call).Next;
            }
            return body.Instructions.First();
        }

        IEnumerable<Instruction> GetCatchInstructions(Instruction catchBlockLeaveInstructions)
        {
            yield return Instruction.Create(OpCodes.Pop);
            yield return Instruction.Create(OpCodes.Nop);
            yield return Instruction.Create(OpCodes.Nop);
            yield return catchBlockLeaveInstructions;
        }
    }
}
