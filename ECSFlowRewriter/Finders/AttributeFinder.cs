using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace ECSFlow.Finder
{
    public class AttributeFinder
    {

        public AttributeFinder()
        {
            Exceptions = new List<TypeReference>();
        }

        public IAssemblyResolver AssemblyResolver { get; set; }

        public AttributeFinder(MethodDefinition method)
        {
            var customAttributes = method.CustomAttributes;
            CustomAttribute attr = null;

            if (customAttributes.ContainsAttribute("ECSFlow.Fody.ExceptionRaiseSiteAttribute"))
            {
                Raising = true;
            }

            if (customAttributes.ContainsAttribute("ECSFlow.Fody.ExceptionChannelAttribute", ref attr))
            {
                Channel = true;

                ImportExceptionTypesFromChannel(method, attr);
            }

            if (customAttributes.ContainsAttribute("ECSFlow.Fody.ExceptionHandlerAttribute"))
            {
                Handler = true;
            }

            if (customAttributes.ContainsAttribute("ECSFlow.Fody.ExceptionInterfaceAttribute"))
            {
                Interface = true;
            }
        }

        private void ImportExceptionTypesFromChannel(MethodDefinition method, CustomAttribute attr)
        {
            // Look for Exceptions Types in ExceptionChannelAttribute
            if (attr != null)
            {
                Exceptions = new List<TypeReference>();
                CustomAttributeArgument[] args = (CustomAttributeArgument[])attr.ConstructorArguments[2].Value;
                foreach (var item in args)
                {
                    Exceptions.Add(method.Module.ImportReference(Type.GetType(item.Value.ToString())));
                }
            }

            ////Import each Exception Type in current Module
            //foreach (var exception in Exceptions)
            //{
            //    var eFlowDefinition = ModuleDefinition.ReadModule(Assembly.GetExecutingAssembly().Location).Assembly;

            //    foreach (var reference in eFlowDefinition.MainModule.AssemblyReferences)
            //    {
            //        try
            //        {
            //            var systemName = AssemblyNameReference.Parse(reference.FullName);
            //            AssemblyDefinition system = new DefaultAssemblyResolver().Resolve(systemName);


            //            // Code to deep copy the reference assembly into the main assembly
            //            var importer = new TypeImporter(system.MainModule, method.Module.Assembly.MainModule);
            //            foreach (var definition in method.Module.Assembly.Modules.SelectMany(x => x.Types).ToArray())
            //            {
            //                importer.Import(definition);
            //            }

            //            var exceptionType = system.MainModule.GetTypes().First(x => x.Name == exception.Name);
            //            method.Module.ImportReference(exceptionType);
            //        }
            //        catch (Exception e) { Console.WriteLine(e.Message); };
            //    }
            //}
        }

        public bool Swallow;
        public bool Raising;
        public bool Channel;
        public bool Handler;
        public bool Interface;
        public List<TypeReference> Exceptions = new List<TypeReference>();
    }
}