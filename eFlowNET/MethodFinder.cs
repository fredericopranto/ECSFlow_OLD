using Mono.Cecil;
using System.Linq;

namespace eFlowNET.Fody
{
    public class MethodFinder
    {
        public MethodFinder(TypeReference exceptionType)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule("eFlowNET.dll");
            TypeDefinition type = module.Types.First(t => t.FullName == "eFlowNET.Fody.GlobalExceptionDefinitions");

            //search:
            foreach (var methodRef in type.Methods)
            {
                foreach (var par in methodRef.Parameters.ToList())
                {
                    if (par.ParameterType.FullName.Equals(exceptionType.FullName))
                    {
                        Found = true;
                        Method = methodRef;
                        //goto search;
                    }
                }
            }
        }

        public bool Found;
        public MethodDefinition Method;
    }
}