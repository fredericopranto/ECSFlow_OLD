using ECSFlowAttributes;
using System;

namespace ECSFlowAttributes
{
    /// <summary>
    /// If an <see cref="Exception"/> occurs in the applied method then flow it explicit.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [CLSCompliant(true)]
    public class ExceptionRaiseSiteAttribute : Attribute, IECSFlowAttribute
    {
        public ExceptionChannelAttribute channel;
        public ExceptionHandlerAttribute handler;
        public string RaiseSiteName;
        public string RaiseSiteTarget;
        public Type Type;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RaiseSite Name"></param>
        /// <param name="RaiseSite Target"></param>
        public ExceptionRaiseSiteAttribute(Type Type, string RaiseSiteName, string RaiseSiteTarget)
        {
            this.Type = Type;
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