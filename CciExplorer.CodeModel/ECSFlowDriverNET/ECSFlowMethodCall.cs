using System.Xml.Serialization;

namespace ECSFlowDriverNET
{
    [XmlRoot("methodCall")]
    public class ECSFlowMethodCall
    {
        [XmlElement("methodSource")]
        public ECSFlowMethod MethodSource { get; set; }
        [XmlElement("methodTarget")]
        public ECSFlowMethod MethodTarget { get; set; }
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
