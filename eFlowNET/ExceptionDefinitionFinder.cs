

using Mono.Cecil;
using Mono.Collections.Generic;
using System.Linq;

namespace eFlowNET.Fody
{
    public class ExceptionDefinitionFinder
    {
        public ExceptionDefinitionFinder(MethodDefinition method)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule("eFlowNET.dll");
            TypeDefinition type = module.Types.First(t => t.FullName == "eFlowNET.Fody.GlobalExceptionDefinitions");
            
            foreach (var item in type.Methods)
            {
                if (item.Name.Equals(method.Name))
                {
                    Inpect = true;
                    CustomAttributes = new Collection<CustomAttribute>(item.CustomAttributes.ToList().FindAll(x => x.AttributeType.Name.Contains("Exception")));
                }
                
                break;
            }
        }

        public bool Inpect;
        public Collection<CustomAttribute> CustomAttributes;
    }
}