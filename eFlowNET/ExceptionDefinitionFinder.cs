

using Mono.Cecil;
using Mono.Collections.Generic;
using System.Linq;

namespace DotNetFlow.Fody
{
    public class ExceptionDefinitionFinder
    {
        public ExceptionDefinitionFinder(MethodDefinition method)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule("DotNetFlow.dll");
            TypeDefinition type = module.Types.First(t => t.FullName == "DotNetFlow.Fody.GlobalExceptionDefinitions");
            
            foreach (var item in type.Methods)
            {
                if (item.Name.Equals(method.Name))
                {
                    Inpect = true;
                    CustomAttributes = item.CustomAttributes;
                }
                
                break;
            }
        }

        public bool Inpect;
        public Collection<CustomAttribute> CustomAttributes;
    }
}