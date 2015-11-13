using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Microsoft.Cci;

namespace eFlowDriverNET
{
    [XmlRoot("method")]
    public class eFlowMethod
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("fullName")]
        public string FullName { get; set; }
        [XmlElement("visibility")]
        public string Visibility { get; set; }
        [XmlElement("qtdTry")]
        public int QtdTry { get; set; }
        [XmlElement("qtdCatch")]
        public int QtdCatch { get; set; }
        [XmlElement("qtdCatchGeneric")]
        public int QtdCatchGeneric { get; set; }
        [XmlElement("qtdCatchSpecialized")]
        public int QtdCatchSpecialized { get; set; }
        [XmlElement("qtdThrow")]
        public int QtdThrow { get; set; }
        [XmlElement("qtdFinally")]
        public int QtdFinally { get; set; }
        [XmlIgnore]
        [IgnoreDataMemberAttribute]
        public IMethodDefinition MethodDefinition { get; set; }

        private readonly List<eFlowMethodException> _MethodExceptions = new List<eFlowMethodException>();

        [XmlArray("methodExceptions")]
        [XmlArrayItem("methodException")]
        public List<eFlowMethodException> MethodExceptions { get { return _MethodExceptions; } }

    }
}
