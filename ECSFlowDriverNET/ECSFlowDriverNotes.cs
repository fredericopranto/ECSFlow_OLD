using Microsoft.Cci;
using System;
using System.Xml;

namespace ECSFlowDriverNET
{
    /// <summary>
    /// Class for generate exception flow paths
    /// </summary>
    public class eFlowNotes
    {
        /// <summary>
        /// 
        /// </summary>
        private static void eFlow2()
        {
            //string assemblyLocator = @"..\..\Input\Ionic.Zip.dll";
            string assemblyLocator = @"..\..\Input\ClassLibraryExemplo.dll";
            MetadataReaderHost module = new PeReader.DefaultHost();
            var assembly = module.LoadUnitFrom(assemblyLocator) as IModule;

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(assemblyLocator);
            DateTime lastModified = fileInfo.LastWriteTime;

            ECSFlowAssembly eFlowAssembly = new ECSFlowAssembly();
            eFlowAssembly.Name = assembly.Name.Value;
            eFlowAssembly.Version = assembly.ModuleIdentity.ContainingAssembly.Version.ToString();
            eFlowAssembly.CreatedAt = lastModified.ToUniversalTime().ToString();
            //TODO: recuperar dinamicamente a linguagem do assembly
            eFlowAssembly.Language = "C#.NET";

            ECSFlowDriverProcess.ProcessAssembly(assembly, ref eFlowAssembly);

            string xml = ECSFlowDriverProcess.XmlSerializeObject(eFlowAssembly);
            //string xml = DataContractSerializeObject(eFlowAssembly);

            XmlDocument origXml = new XmlDocument();
            origXml.LoadXml(xml);
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<list></list>");
            XmlNode rootNode = newXml.ImportNode(origXml.DocumentElement, true);
            newXml.DocumentElement.AppendChild(rootNode);
            newXml.Save(String.Join(string.Empty, @"..\..\Output\", eFlowAssembly.Name, "-", eFlowAssembly.Version, "2.xml"));

        }

        public void JavaReferencesTests()
        {
            //Java Method Call
            //string novoxml = new global::Main().getReferencedXMLPublic(xml);


            //string xml = DataContractSerializeObject(eFlowAssembly);

            //string json = JsonConvert.SerializeObject(eFlowAssembly);
            //string xml = JsonConvert.DeserializeXNode(json, "Root").ToString();

            //var jsonSerializer = new JsonSerializer
            //{
            //    NullValueHandling = NullValueHandling.Ignore,
            //    MissingMemberHandling = MissingMemberHandling.Ignore,
            //    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            //};
            //var sb = new StringBuilder();
            //using (var sw = new StringWriter(sb))
            //using (var jtw = new JsonTextWriter(sw))
            //    jsonSerializer.Serialize(jtw, eFlowAssembly);
            //string json  = sb.ToString();
            //string xml = JsonConvert.DeserializeXNode(json, "Root").ToString();
        }
    }
}
