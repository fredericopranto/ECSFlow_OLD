using System;
using System.Collections.Generic;
using Microsoft.Cci;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace ECSFlowDriverNET
{
    [XmlRoot("assembly")]
    [DataContract(Name = "assembly")]
    public class ECSFlowAssembly
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
        [XmlElement("tryCount")]
        public int TryCount { get; set; }
        [XmlElement("catchCount")]
        public int CatchCount
        {   get {   return CatchGenericCount + CatchSpecializedCount;} }
        [XmlElement("catchGenericCount")]
        [XmlIgnore]
        public int CatchGenericCount { get; set; }
        [XmlElement("catchSpecializedCount")]
        [XmlIgnore]
        public int CatchSpecializedCount { get; set; }
        [XmlElement("throwCount")]
        public int ThrowCount { get; set; }
        [XmlElement("finallyCount")]
        public int FinallyCount { get; set; }
        [XmlIgnore]
        public IModule Assembly { get; set; }

        private readonly List<ECSFlowAssembly> _Assemblies = new List<ECSFlowAssembly>();
        private readonly List<ECSFlowException> _Exceptions = new List<ECSFlowException>();
        private readonly List<ECSFlowType> _Types = new List<ECSFlowType>();
        private readonly List<ECSFlowMethodCall> _MethodCalls = new List<ECSFlowMethodCall>();

        [XmlIgnore]
        [XmlArray("assemblies")]
        [XmlArrayItem("assembly")]
        public List<ECSFlowAssembly> Assemblies { get { return _Assemblies; } }
        [XmlArray("exceptions")]
        [XmlArrayItem("exception")]
        public List<ECSFlowException> Exceptions { get { return _Exceptions; } }
        [XmlArray("types")]
        [XmlArrayItem("type")]
        public List<ECSFlowType> Types { get { return _Types; } }
        [XmlArray("methodCalls")]
        [XmlArrayItem("methodCall")]
        public List<ECSFlowMethodCall> MethodCalls { get { return _MethodCalls; } }

        public void RegisterException(string pExeception)
        {
            //TODO: Melhorar o código
            if (!pExeception.Equals("System.Object")) // Validação do Catch não tipado
                if (this.Exceptions.FindAll(e => e.Name.Equals(pExeception)).Count == 0)
                {
                    ECSFlowException eFlowException = new ECSFlowException();
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
