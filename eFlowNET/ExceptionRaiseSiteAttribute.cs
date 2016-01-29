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
        public string raiseSite;
        
        public ExceptionRaiseSiteAttribute(string alias)
        {
            this.alias = alias;
        }

        public ExceptionRaiseSiteAttribute(string alias, string raiseSite)
        {
            this.alias = alias;
            this.raiseSite = raiseSite;
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