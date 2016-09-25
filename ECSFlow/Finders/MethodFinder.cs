using Mono.Cecil;
using System;
using System.Linq;

namespace ECSFlow.Fody
{
    /// <summary>
    /// 
    /// </summary>
    public class MethodFinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exceptionType"></param>
        public MethodFinder(TypeReference exceptionType)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule("ECSFlow.dll");
            TypeDefinition type = module.Types.First(t => t.FullName == "AssemblyToProcessHander");

            foreach (var methodRef in type.Methods)
            {
                foreach (var par in methodRef.Parameters.ToList())
                {
                    if (par.ParameterType.FullName.Equals(exceptionType.FullName))
                    {
                        Found = true;
                        MethodDefinition = methodRef;
                        TypeReference = type;
                        MethodReference = methodRef.GetElementMethod();
                        Console.WriteLine("Method found");
                        break;
                    }
                }
            }
        }

        public bool Found;
        public MethodDefinition MethodDefinition;
        public TypeReference TypeReference;
        public MethodReference MethodReference;
    }
}