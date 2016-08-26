﻿using System;

namespace eFlowNET.Fody
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionHandlerAttribute : Attribute
    {
        public string[] channelList;
        public string HandlingSite;
        public string[] exceptionList;
        public HandlerDelegate handlerDelegate;


        public ExceptionHandlerAttribute(string[] channelList, string HandlingSite, Type delegateType, string delegateName)
        {
            this.channelList = channelList;
            this.HandlingSite = HandlingSite;
            handlerDelegate = (HandlerDelegate)Delegate.CreateDelegate(delegateType, delegateType.GetMethod(delegateName));
        }

        public ExceptionHandlerAttribute(string[] channelList, string HandlingSite, string[] exceptionList, 
            Type delegateType, string delegateName) : this(channelList, HandlingSite, delegateType, delegateName)
        {
            this.exceptionList = exceptionList;
        }

        public ExceptionHandlerAttribute(string channelList, string HandlingSite, Type delegateType, string delegateName)
            : this(new string[] { channelList }, HandlingSite, delegateType, delegateName)
        { }
    }
}