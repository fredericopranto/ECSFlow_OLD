using Mono.Cecil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace eFlowNET.Fody
{
    public class AttributeFinder
    {
        public IAssemblyResolver AssemblyResolver { get; set; }

        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            CustomAttribute attr = null;
            
            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionRaiseSiteAttribute"))
            {
                Raising = true;
            }

            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionChannelAttribute", ref attr))
            {
                Channel = true;

                // Look for Exceptions Types in ExceptionChannelAttribute
                if (attr != null)
                {
                    Exceptions = new List<TypeReference>();
                    CustomAttributeArgument[] args = (CustomAttributeArgument[])
                            ((CustomAttributeArgument)attr.ConstructorArguments[1]).Value;
                    foreach (var item in args)
                    {
                        Exceptions.Add((TypeReference)item.Value);
                    }
                }

                //Import each Exception Type in current Module
                foreach (var item in Exceptions)
                {
                    var assemblyResolver = new MockAssemblyResolver
                    {
                        Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    };

                    var msCoreLibDefinition = assemblyResolver.Resolve("System.Data");
                    method.Module.ImportReference(msCoreLibDefinition.MainModule.Types
                        .First(x => x.Name == item.Name));
                }
                
            }

            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionHandlerAttribute"))
            {
                Handler = true;
            }

            if (customAttributes.ContainsAttribute("eFlowNET.Fody.ExceptionInterfaceAttribute"))
            {
                Interface = true;
            }
        }

        public bool Swallow;
        public bool Raising;
        public bool Channel;
        public bool Handler;
        public bool Interface;
        public List<TypeReference> Exceptions { get; set; }
    }
}