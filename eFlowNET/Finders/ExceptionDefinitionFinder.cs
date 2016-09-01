using ECSFlow.Attributes;
using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

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
        /// <param name="method"></param>
        public ExceptionDefinitionFinder(MethodDefinition method)
        {
            ModuleDefinition module = ModuleDefinition.ReadModule("ECSFlow.dll");

            IQueryable<CustomAttribute> rsites = (IQueryable<CustomAttribute>) from t in method.Resolve().CustomAttributes.AsQueryable()
                         where t.AttributeType.Resolve().FullName == typeof(ExceptionRaiseSiteAttribute).FullName
                            select t;

            IQueryable<CustomAttribute> channels = from t in method.Resolve().CustomAttributes.AsQueryable()
                                                   where t.AttributeType.Resolve().FullName == typeof(ExceptionChannelAttribute).FullName
                                                   select t;

            IQueryable<CustomAttribute> handlers = from t in method.Resolve().CustomAttributes.AsQueryable()
                                                   where t.AttributeType.Resolve().FullName == typeof(ExceptionHandlerAttribute).FullName
                                                   select t;

            IQueryable<CustomAttribute> interfaces = from t in method.Resolve().CustomAttributes.AsQueryable()
                                                     where t.AttributeType.Resolve().FullName == typeof(ExceptionInterfaceAttribute).FullName
                                                     select t;
                
            foreach (var item in rsites)
            {
                IQueryable<CustomAttribute> match = from t in method.Resolve().CustomAttributes.AsQueryable()
                                                    where t.AttributeType.FullName == typeof(ExceptionRaiseSiteAttribute).FullName
                                                    select t;

                if (match != null)
                {
                    Inpect = true;
                    CustomAttributes.Add(item);
                }

                break;
            }
        }

        public bool Inpect;
        public Collection<Mono.Cecil.CustomAttribute> CustomAttributes;
    }
}