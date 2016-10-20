using System.Xml.Serialization;

namespace ECSFlowDriverNET
{
    [XmlRoot("methodException")]
    public class ECSFlowMethodException
    {
        //[XmlElement("throwsIntoCatch")]
        //public eFlowThrowsIntoCatch ThrowsIntoCatch { get; set; }
        //[DataMember(Name = "exception")]
        //public eFlowException  ExceptionReference { get; set; }
        [XmlElement("kind")]
        public string Kind { get; set; }
        [XmlElement("isGeneric")]
        public bool IsGeneric { get; set; }
        [XmlElement("startOffSet")]
        public string StartOffSet { get; set; }
        [XmlElement("endOffSet")]
        public string EndOffSet { get; set; }
    }

    [XmlRoot("throwsIntoCatch")]
    public class eFlowThrowsIntoCatch
    {
        [XmlElement("string")]
        public string String { get; set; }
    }
}
