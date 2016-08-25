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
        public string raiseSite;
        public string[] exceptionList;
        public HandlerDelegate handlerDelegate;


        public ExceptionHandlerAttribute(string[] channelList, string raiseSite, Type delegateType, string delegateName)
        {
            this.channelList = channelList;
            this.raiseSite = raiseSite;
            handlerDelegate = (HandlerDelegate)Delegate.CreateDelegate(delegateType, delegateType.GetMethod(delegateName));
        }

        public ExceptionHandlerAttribute(string[] channelList, string[] exceptionList, string raiseSite, RaiseSiteScope raiseSiteScope,
            Type delegateType, string delegateName) : this(channelList, raiseSite, delegateType, delegateName)
        {
            this.exceptionList = exceptionList;
        }

        public ExceptionHandlerAttribute(string channelList, string raiseSite, Type delegateType, string delegateName)
            : this(new string[] { channelList }, raiseSite, delegateType, delegateName)
        { }
    }
}