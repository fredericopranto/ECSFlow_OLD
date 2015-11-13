using System;

namespace eFlowNET.Fody
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class ExceptionHandlerAttribute : Attribute
    {
        public string _channel;
        public Type _exceptionType;
        public string _handlerName;

        public ExceptionHandlerAttribute(string channel, Type exceptionType, string handlerName)
        {
            _channel = channel;
            _exceptionType = exceptionType;
            _handlerName = handlerName;
        }
    }
}