using Mono.Cecil;
using System;
using System.Linq;

namespace ECSFlow.Finder
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
        public MethodFinder(TypeReference exceptionType, TypeDefinition MappingType)
        {
            foreach (var methodRef in MappingType.Methods)
            {
                foreach (var par in methodRef.Parameters.ToList())
                {
                    if (par.ParameterType.FullName.Equals(exceptionType.FullName))
                    {
                        Found = true;
                        MethodDefinition = methodRef;
                        TypeReference = MappingType;
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