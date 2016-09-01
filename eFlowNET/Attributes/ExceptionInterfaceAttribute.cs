using ECSFlow.Attributes;
using System;
using System.Collections.Generic;

namespace ECSFlow.Fody
{
    /// <summary>
    /// An explicit exception channel (channel, for short) is an abstract duct through which exceptions 
    /// flow from a raising site to a handling site.
    /// 
    /// When a class or program cannot handle all the exceptions that flow through an explicit
    /// exception channel, it is necessary to declare these exceptions in the channel’s exception
    /// interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionInterfaceAttribute : Attribute, IECSFlowAttribute
    {
        public InterfaceScope scope;
        public string exception;
        public string alias;
        public string channel;
        public string raiseSiteName;
        public bool isNamespace;

        public ExceptionInterfaceAttribute(string channel, string raiseSiteName, bool isNamespace, string exception = null)
        {
            this.exception = exception;
            this.channel = channel;
            this.raiseSiteName = raiseSiteName;
            this.isNamespace = isNamespace;
        }
    }

    public enum InterfaceScope
    {
        Namespace,
        Channel
    }
}