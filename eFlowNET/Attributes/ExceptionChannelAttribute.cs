using System;
using System.Collections.Generic;

namespace eFlowNET.Fody
{
    /// <summary>
    /// An explicit exception channel (channel, for short) is an abstract duct through which exceptions 
    /// flow from a raising site to a handling site.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionChannelAttribute : Attribute
    {
        public ExceptionRaiseSiteAttribute RaiseSite;
        public string channelName;
        public string[] raiseSiteNameList;
        public bool isSubsumption;
        public string[] exceptionList;

        public ExceptionChannelAttribute(string[] exceptionList)
        {
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(string[] exceptionList, bool isSubsumption, string[] raiseSiteNameList)
        {
            this.raiseSiteNameList = raiseSiteNameList;
            this.isSubsumption = isSubsumption;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(string channelName, string[] exceptionList, bool isSubsumption, string[] raiseSiteNameList)
        {
            this.channelName = channelName;
            this.raiseSiteNameList = raiseSiteNameList;
            this.isSubsumption = isSubsumption;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(string channelName, string exception, bool isSubsumption, string raiseSiteName)
            : this(channelName, new string[]{ exception }, isSubsumption, new string[] { raiseSiteName })
        { }

        public ExceptionChannelAttribute(string channelName, string[] exceptionList, bool isSubsumption, string raiseSiteName)
            : this(channelName, exceptionList, isSubsumption, new string[] { raiseSiteName })
        { }
    }
}