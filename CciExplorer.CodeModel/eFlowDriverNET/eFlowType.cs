using System.Collections.Generic;
using System.Xml.Serialization;

namespace eFlowDriverNET
{
    [XmlRoot("type")]
    public class eFlowType
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("fullName")]
        public string FullName { get; set; }
        [XmlElement("kind")]
        public string Kind { get; set; }

        private readonly List<eFlowMethod> _Methods = new List<eFlowMethod>();

        [XmlArray("methods")]
        [XmlArrayItem("method")]
        public List<eFlowMethod> Methods { get { return _Methods; } }
    }
}
