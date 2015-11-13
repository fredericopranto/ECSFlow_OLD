using System;
using System.Collections.Generic;

namespace DotNetFlow.Fody
{
    /// <summary>
    /// An explicit exception channel (channel, for short) is an abstract duct through which exceptions 
    /// flow from a raising site to a handling site.
    /// 
    /// When a class or program cannot handle all the exceptions that flow through an explicit
    /// exception channel, it is necessary to declare these exceptions in the channel’s exception
    /// interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class ExceptionInterfaceAttribute : Attribute
    {
        public InterfaceScope scope;
        public Type exception;
        public string alias;

        public ExceptionInterfaceAttribute(Type exceptionType)
        {
            exception = exceptionType;
        }

        
    }

    public enum InterfaceScope
    {
        Namespace,
        Channel
    }
}
