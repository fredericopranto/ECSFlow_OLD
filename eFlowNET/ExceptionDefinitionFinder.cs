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
            //TypeDefinition type = module.Types.First(t => t.FullName == "eFlowNET.Fody.GlobalExceptionInfo");
            System.Collections.Generic.IEnumerable<CustomAttribute> rsites =
                module.Assembly.CustomAttributes.Where(t => t.AttributeType.Name.Equals("ExceptionRaiseSiteAttribute"));

            foreach (var item in rsites)
            {
                //if (item.ConstructorArguments.First(t =>  t.Value.Equals(method.Name)))
                //{
                //    Inpect = true;
                //    CustomAttributes = new Collection<CustomAttribute>(item.CustomAttributes.ToList().FindAll(x => x.AttributeType.Name.Contains("Exception")));
                //}
                
                break;
            }
        }

        public bool Inpect;
        public Collection<CustomAttribute> CustomAttributes;
    }
}