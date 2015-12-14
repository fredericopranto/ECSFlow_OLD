using System;

namespace eFlowNET.Fody
{
    /// <summary>
    /// If an <see cref="Exception"/> occurs in the applied method then flow it explicit.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionRaiseSiteAttribute : Attribute
    {
        public ExceptionChannelAttribute channel;
        public ExceptionHandlerAttribute handler;
        public string alias;
        public string methodName;
        public RaiseSiteScope raiseSiteScope;

        public ExceptionRaiseSiteAttribute(string alias)
        {
            this.alias = alias;
        }

        public ExceptionRaiseSiteAttribute(string alias, string methodName, RaiseSiteScope raiseSiteScope = RaiseSiteScope.Method)
        {
            this.alias = alias;
            this.methodName = methodName;
            this.raiseSiteScope = raiseSiteScope;
        }
    }

    public enum RaiseSiteScope
    {
        Namespace,
        Method,
        Class,
        ByPass
    }
}