using System;
using System.Collections.Generic;

namespace eFlowNET.Fody
{
    /// <summary>
    /// An explicit exception channel (channel, for short) is an abstract duct through which exceptions 
    /// flow from a raising site to a handling site.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class ExceptionChannelAttribute : Attribute
    {
        public ExceptionRaiseSiteAttribute RaiseSite;
        public string name;
        public Type[] exceptions;
        

        public ExceptionChannelAttribute(string channelName, params Type[] exceptionList)
        {
            name = channelName;
            exceptions = exceptionList;
        }
    }
}
