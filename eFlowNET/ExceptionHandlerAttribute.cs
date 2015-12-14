using System;

namespace eFlowNET.Fody
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionHandlerAttribute : Attribute
    {
        public string[] channelList;
        public string component;
        public RaiseSiteScope raiseSiteScope;

        public ExceptionHandlerAttribute(string[] channelList, string component, RaiseSiteScope raiseSiteScope = RaiseSiteScope.Class)
        {
            this.channelList = channelList;
            this.component = component;
            this.raiseSiteScope = raiseSiteScope;
        }

        public ExceptionHandlerAttribute(string channel, string component, RaiseSiteScope raiseSiteScope = RaiseSiteScope.Class)
            : this(new string[] { channel }, component, raiseSiteScope)
        { }
    }
}