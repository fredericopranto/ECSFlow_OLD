using System.Xml.Serialization;

namespace eFlowDriverNET
{
    [XmlRoot("methodCall")]
    public class eFlowMethodCall
    {
        [XmlElement("methodSource")]
        public eFlowMethod MethodSource { get; set; }
        [XmlElement("methodTarget")]
        public eFlowMethod MethodTarget { get; set; }
        [XmlElement("offSet")]
        public string OffSet { get; set; }
        [XmlElement("order")]
        public string Order { get; set; }
    }

    public class eFlowAttributeReference
    {
        [XmlAttribute("reference")]
        public int Reference;
    }
}
