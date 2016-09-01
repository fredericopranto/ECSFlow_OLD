using ECSFlow.Attributes;
using System;

namespace ECSFlow.Fody
{
    /// <summary>
    /// If an <see cref="Exception"/> occurs in the applied method then flow it explicit.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionRaiseSiteAttribute : Attribute, IECSFlowAttribute
    {
        public ExceptionChannelAttribute channel;
        public ExceptionHandlerAttribute handler;
        public string RaiseSiteName;
        public string RaiseSiteTarget;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RaiseSite Name"></param>
        /// <param name="RaiseSite Target"></param>
        public ExceptionRaiseSiteAttribute(string RaiseSiteName, string RaiseSiteTarget)
        {
            this.RaiseSiteName = RaiseSiteName;
            this.RaiseSiteTarget = RaiseSiteTarget;
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