using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Microsoft.Cci;

namespace ECSFlowDriverNET
{
    [XmlRoot("method")]
    public class ECSFlowMethod
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

        private readonly List<ECSFlowMethodException> _MethodExceptions = new List<ECSFlowMethodException>();

        [XmlArray("methodExceptions")]
        [XmlArrayItem("methodException")]
        public List<ECSFlowMethodException> MethodExceptions { get { return _MethodExceptions; } }

    }
}
