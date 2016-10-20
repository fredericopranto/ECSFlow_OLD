using System.Collections.Generic;
using System.Xml.Serialization;

namespace ECSFlowDriverNET
{
    [XmlRoot("type")]
    public class ECSFlowType
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("fullName")]
        public string FullName { get; set; }
        [XmlElement("kind")]
        public string Kind { get; set; }

        private readonly List<ECSFlowMethod> _Methods = new List<ECSFlowMethod>();

        [XmlArray("methods")]
        [XmlArrayItem("method")]
        public List<ECSFlowMethod> Methods { get { return _Methods; } }
    }
}
