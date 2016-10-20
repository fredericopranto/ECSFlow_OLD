using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ECSFlowDriverNET
{
    [XmlRoot("exception")]
    [DataContract(Name = "exception", Namespace = "")]
    public class ECSFlowException
    {
        public ECSFlowException(Type ExceptionFinally)
        {
            this.BaseName = ExceptionFinally.ToString();
            this.Name = ExceptionFinally.Name;
        }

        public ECSFlowException()
        {
            // TODO: Complete member initialization
        }

        [XmlElement("name")]
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [XmlElement("basename")]
        [DataMember(Name = "baseName")]
        public string BaseName { get; set; }
    }
}
