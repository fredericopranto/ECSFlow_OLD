using ECSFlow.Attributes;
using System;

[assembly: CLSCompliant(true)]
namespace ECSFlow.Fody
{
    /// <summary>
    /// An explicit exception channel (channel, for short) is an abstract duct through which exceptions 
    /// flow from a raising site to a handling site.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public class ExceptionChannelAttribute : Attribute, IECSFlowAttribute
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

        public ExceptionChannelAttribute(string[] exceptionList, string[] raiseSiteNameList , bool isSubsumption = false)
        {
            this.raiseSiteNameList = raiseSiteNameList;
            this.isSubsumption = isSubsumption;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(string channelName, string[] exceptionList, string[] raiseSiteNameList, bool isSubsumption = false)
        {
            this.channelName = channelName;
            this.raiseSiteNameList = raiseSiteNameList;
            this.isSubsumption = isSubsumption;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(string channelName, string exception, string raiseSiteName, bool isSubsumption = false)
            : this(channelName, new string[]{ exception }, new string[] { raiseSiteName }, isSubsumption)
        { }

        public ExceptionChannelAttribute(string channelName, string[] exceptionList, string raiseSiteName, bool isSubsumption = false)
            : this(channelName, exceptionList, new string[] { raiseSiteName }, isSubsumption)
        { }
    }
}