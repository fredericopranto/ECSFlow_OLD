using ECSFlow.Attributes;
using System;

namespace ECSFlow.Fody
{
    /// <summary>
    /// An explicit exception channel (channel, for short) is an abstract duct through which exceptions 
    /// flow from a raising site to a handling site.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [CLSCompliant(true)]
    public class ExceptionChannelAttribute : Attribute, IECSFlowAttribute
    {
        public string channelName;
        public string[] raiseSiteNameList;
        public bool isSubsumption;
        public string[] exceptionList;
        public Type Type;

        public ExceptionChannelAttribute()
        {
        }

        public ExceptionChannelAttribute(Type Type, string[] exceptionList)
        {
            this.Type = Type;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(Type Type, string[] exceptionList, string[] raiseSiteNameList , bool isSubsumption = false)
        {
            this.Type = Type;
            this.raiseSiteNameList = raiseSiteNameList;
            this.isSubsumption = isSubsumption;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(Type Type, string channelName, string[] exceptionList, string[] raiseSiteNameList, bool isSubsumption = false)
        {
            this.Type = Type;
            this.channelName = channelName;
            this.raiseSiteNameList = raiseSiteNameList;
            this.isSubsumption = isSubsumption;
            this.exceptionList = exceptionList;
        }

        public ExceptionChannelAttribute(Type Type, string channelName, string exception, string raiseSiteName, bool isSubsumption = false)
            : this(Type, channelName, new string[]{ exception }, new string[] { raiseSiteName }, isSubsumption)
        { }

        public ExceptionChannelAttribute(Type Type, string channelName, string[] exceptionList, string raiseSiteName, bool isSubsumption = false)
            : this(Type, channelName, exceptionList, new string[] { raiseSiteName }, isSubsumption)
        { }
    }
}