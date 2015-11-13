using System;

namespace eFlowNET.Fody
{
    /// <summary>
    /// If an <see cref="Exception"/> occurs in the applied method then flow it explicit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class ExceptionRaiseSiteAttribute : Attribute
    {
        public ExceptionChannelAttribute channel;
        public ExceptionHandlerAttribute handler;
        public string alias;
        
        public ExceptionRaiseSiteAttribute(string siteAlias)
        {
            alias = siteAlias;
        }
    }
}