using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Linq;

namespace ECSFlow.Fody
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionDefinitionFinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Method"></param>
        public ExceptionDefinitionFinder(MethodDefinition Method)
        {
            ModuleDefinition ECSFlowModule = ModuleDefinition.ReadModule("ECSFlowRewriter.dll");

            IQueryable<CustomAttribute> rsites = from t in ECSFlowModule.Assembly.CustomAttributes.AsQueryable()
                                                 where t.AttributeType.Resolve().FullName == typeof(ExceptionRaiseSiteAttribute).FullName
                                                 && t.ConstructorArguments.Any(item => item.Value.ToString().Equals("AssemblyToProcessMapping"))
                                                 select t;

            IQueryable<CustomAttribute> channels = from t in ECSFlowModule.Assembly.CustomAttributes.AsQueryable()
                                                   where t.AttributeType.Resolve().FullName == typeof(ExceptionChannelAttribute).FullName
                                                   && t.ConstructorArguments.Any(item => item.Value.ToString().Equals("AssemblyToProcessMapping"))
                                                   select t;

            IQueryable<CustomAttribute> handlers = from t in ECSFlowModule.Assembly.CustomAttributes.AsQueryable()
                                                   where t.AttributeType.Resolve().FullName == typeof(ExceptionHandlerAttribute).FullName
                                                   && t.ConstructorArguments.Any(item => item.Value.ToString().Equals("AssemblyToProcessMapping"))
                                                   select t;

            IQueryable<CustomAttribute> interfaces = from t in ECSFlowModule.Assembly.CustomAttributes.AsQueryable()
                                                     where t.AttributeType.Resolve().FullName == typeof(ExceptionInterfaceAttribute).FullName
                                                     && t.ConstructorArguments.Any(item => item.Value.ToString().Equals("AssemblyToProcessMapping"))
                                                     select t;

            var matchRaiseSite = from t in rsites
                                 where t.ConstructorArguments.Any(item => item.Value.ToString().Equals(String.Concat(Method.DeclaringType.Name, ".", Method.Name)))
                                 select t;


            var matchChannel = from t in channels
                               where t.ConstructorArguments.Any(item => ((item.Value.GetType().IsArray)))
                               select t;


            foreach (var item in matchRaiseSite)
            {
                Inpect = true;
                CustomAttributes.Add(item);
                Console.WriteLine("Custom Attributes added" + matchRaiseSite);
                Method.CustomAttributes.Add(item);
                

            }

            foreach (var item in matchChannel)
            {
                //Inpect = true;
                //CustomAttributes.Add(item);
                //Console.WriteLine("Custom Attributes added" + matchChannel);

                //MethodDefinition methodDefinition = Method;
                //var module = methodDefinition.DeclaringType.Module;
                //var attr = module.ImportReference(typeof(ExceptionChannelAttribute));

                //foreach (var con in attr.Resolve().GetType().GetConstructors())
                //{
                //    module.ImportReference(con);
                //}


                //var sampleDll = AssemblyDefinition.ReadAssembly("ECSFlowRewriter.dll");
                //var targetExe = Method.DeclaringType.Module.Assembly;

                //var sampleClass = sampleDll.MainModule.GetType("ECSFlow.Fody.ExceptionChannelAttribute");
                //var sampleClassCtor = sampleClass.Methods.First(m => m.IsConstructor);

                //var ctorReference = targetExe.MainModule.ImportReference(sampleClassCtor);

                //CustomAttributes.Add(new CustomAttribute(ctorReference));

                //foreach (var type in targetExe.MainModule.Types)
                //    type.CustomAttributes.Add(new CustomAttribute(ctorReference));
            }
        }

        public bool Inpect;
        public Collection<Mono.Cecil.CustomAttribute> CustomAttributes = new Collection<CustomAttribute>();
    }
}