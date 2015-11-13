using System;
using System.Collections.Generic;
using Microsoft.Cci;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace eFlowDriverNET
{
    [XmlRoot("assembly")]
    [DataContract(Name = "assembly")]
    public class eFlowAssembly
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("version")]
        public string Version { get; set; }
        [XmlElement("createdAt")]
        public string CreatedAt { get; set; }
        [XmlElement("language")]
        public string Language { get; set; }
        [XmlElement("analyzer")]
        public bool Analyzer { get; set; }
        [XmlIgnore]
        public IModule Assembly { get; set; }

        private readonly List<eFlowAssembly> _Assemblies = new List<eFlowAssembly>();
        private readonly List<eFlowException> _Exceptions = new List<eFlowException>();
        private readonly List<eFlowType> _Types = new List<eFlowType>();
        private readonly List<eFlowMethodCall> _MethodCalls = new List<eFlowMethodCall>();

        [XmlIgnore]
        [XmlArray("assemblies")]
        [XmlArrayItem("assembly")]
        public List<eFlowAssembly> Assemblies { get { return _Assemblies; } }
        [XmlArray("exceptions")]
        [XmlArrayItem("exception")]
        public List<eFlowException> Exceptions { get { return _Exceptions; } }
        [XmlArray("types")]
        [XmlArrayItem("type")]
        public List<eFlowType> Types { get { return _Types; } }
        [XmlArray("methodCalls")]
        [XmlArrayItem("methodCall")]
        public List<eFlowMethodCall> MethodCalls { get { return _MethodCalls; } }

        public void RegisterException(string pExeception)
        {
            //TODO: Melhorar o código
            if (!pExeception.Equals("System.Object")) // Validação do Catch não tipado
                if (this.Exceptions.FindAll(e => e.Name.Equals(pExeception)).Count == 0)
                {
                    eFlowException eFlowException = new eFlowException();
                    try
                    {
                        eFlowException.Name = Type.GetType(pExeception).ToString();
                    }
                    catch (Exception)
                    {
                        //TODO: Generalizar esta solução (Percorrer todas as referencias da dll principal)
                        //eFlowException.Name = Type.GetType(pExeception + ", " + this.Name).ToString();
                        eFlowException.Name = pExeception;
                        
                    }
                    try
                    {
                        eFlowException.BaseName = Type.GetType(pExeception).BaseType.ToString();
                    }
                    catch (Exception)
                    {
                        //TODO: Generalizar esta solução (Percorrer todas as referencias da dll principal)
                        //eFlowException.BaseName = Type.GetType(pExeception + ", " + this.Name).BaseType.ToString();
                        eFlowException.BaseName = pExeception;
                    }

                    this.Exceptions.Add(eFlowException);
                }
        }
    }
}
