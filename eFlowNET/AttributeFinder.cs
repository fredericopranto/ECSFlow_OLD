using Mono.Cecil;
using System;
using System.Collections.Generic;
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
                    CustomAttributeArgument[] args = (CustomAttributeArgument[])attr.ConstructorArguments[1].Value;
                    foreach (var item in args)
                    {
                        Exceptions.Add((TypeReference)item.Value);
                    }
                }

                //Import each Exception Type in current Module
                foreach (var exception in Exceptions)
                {
                    var eFlowDefinition = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location).Assembly;

                    foreach (var reference in eFlowDefinition.MainModule.AssemblyReferences)
                    {
                        try
                        {
                            var systemName = AssemblyNameReference.Parse(reference.FullName);
                            AssemblyDefinition system = new DefaultAssemblyResolver().Resolve(systemName);


                            // Code to deep copy the reference assembly into the main assembly
                            var importer = new TypeImporter(system.MainModule, method.Module.Assembly.MainModule);
                            foreach (var definition in method.Module.Assembly.Modules.SelectMany(x => x.Types).ToArray())
                            {
                                importer.Import(definition);
                            }

                            var exceptionType = system.MainModule.GetTypes().First(x => x.Name == exception.Name);
                            method.Module.ImportReference(exceptionType);
                        }
                        catch (Exception e) { Console.WriteLine(e.Message); };
                    }
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
        public List<TypeReference> Exceptions { get; set; } = new List<TypeReference>();
    }
}