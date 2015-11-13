using Microsoft.Cci;
using Microsoft.Cci.Immutable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using TourreauGilles.CciExplorer.CSharp;

namespace eFlowDriverNET
{
    public class Program
    {
        /// <summary>
        /// Método Main - Executa a aplicação
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            eFlow();
            //eFlow2();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void eFlow()
        {
            string assemblyPath;

            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
            assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");

            MetadataReaderHost module = new PeReader.DefaultHost();
            var assembly = module.LoadUnitFrom(assemblyPath) as IModule;

            //foreach (IAssemblyReference reference in assembly.AssemblyReferences)
            //    assembly = module.LoadAssembly(assembly.CoreAssemblySymbolicIdentity);

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(assemblyPath);
            DateTime lastModified = fileInfo.LastWriteTime;

            eFlowAssembly eFlowAssembly = new eFlowAssembly();
            //eFlowAssembly.Assembly = assembly;
            eFlowAssembly.Name = assembly.Name.Value;
            eFlowAssembly.Version = assembly.ModuleIdentity.ContainingAssembly.Version.ToString();
            eFlowAssembly.CreatedAt = lastModified.ToString("yyyy-MM-dd HH:mm:ss.fff UTC");
            //TODO: recuperar dinamicamente a linguagem do assembly
            eFlowAssembly.Language = "C#.NET";
            CSharpSourceGenerator Generator = new CSharpSourceGenerator(module, eFlowAssembly);

            //Referencias
            foreach (IAssemblyReference reference in assembly.AssemblyReferences)
            {
                eFlowAssembly newEFlowAssembly = new eFlowAssembly();
                newEFlowAssembly.Name = reference.Name.ToString();
                newEFlowAssembly.Version = reference.ModuleIdentity.ContainingAssembly.Version.ToString();
                newEFlowAssembly.Language = "C#.NET";
                eFlowAssembly.Assemblies.Add(newEFlowAssembly);
            }

            // Inspeciona o Assembly
            Generator.Visit(assembly);
            
            // Gera fluxos das chamadas dos métodos
            GenerateMethodCalls(eFlowAssembly);

            // Serializa Objeto
            string xml = XmlSerializeObject(eFlowAssembly);


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
            
            XmlDocument origXml = new XmlDocument();
            origXml.LoadXml(xml);
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<list></list>");
            XmlNode rootNode = newXml.ImportNode(origXml.DocumentElement, true);
            newXml.DocumentElement.AppendChild(rootNode);
            newXml.Save(String.Join(string.Empty, @"..\..\Output\", eFlowAssembly.Name, "-", eFlowAssembly.Version, ".xml"));
        }

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

            eFlowAssembly eFlowAssembly = new eFlowAssembly();
            eFlowAssembly.Name = assembly.Name.Value;
            eFlowAssembly.Version = assembly.ModuleIdentity.ContainingAssembly.Version.ToString();
            eFlowAssembly.CreatedAt = lastModified.ToUniversalTime().ToString();
            //TODO: recuperar dinamicamente a linguagem do assembly
            eFlowAssembly.Language = "C#.NET";

            ProcessAssembly(assembly, ref eFlowAssembly);

            string xml = XmlSerializeObject(eFlowAssembly);
            //string xml = DataContractSerializeObject(eFlowAssembly);

            XmlDocument origXml = new XmlDocument();
            origXml.LoadXml(xml);
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<list></list>");
            XmlNode rootNode = newXml.ImportNode(origXml.DocumentElement, true);
            newXml.DocumentElement.AppendChild(rootNode);
            newXml.Save(String.Join(string.Empty, @"..\..\Output\", eFlowAssembly.Name, "-", eFlowAssembly.Version, "2.xml"));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsGeneric(ITypeReference ExceptionType)
        {
            if (ExceptionType.ResolvedType.ToString() == "Exception" ||
                ExceptionType.ResolvedType.ToString() == "SystemException" ||
                ExceptionType.ResolvedType.ToString() == "ApplicationException" ||
                ExceptionType.ResolvedType.ToString() == "java.lang.Exception" ||
                ExceptionType.ResolvedType.ToString() == "java.lang.Error")
                return true;
            else
                return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static string KindType(INamedTypeDefinition t)
        {
            string str = "";

            if (t.IsAbstract)
                str = "Abstract|";

            if (t.IsClass)
                str += "Class|";

            if (t.IsInterface)
                str += "Interface|";

            if (t.IsReferenceType)
                str += "ReferenceType|";

            if (t.IsRuntimeSpecial)
                str += "RuntimeSpecial|";

            if (t.IsStatic)
                str += "Static|";

            if (t.IsEnum)
                str += "Enum|";

            if (t.IsSealed)
                str += "Sealed|";

            if (t.IsSpecialName)
                str += "SpecialName|";

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="eFlowAssembly"></param>
        private static void ProcessAssembly(IModule assembly, ref eFlowAssembly eFlowAssembly)
        {
            try
            {
                PeReader.DefaultHost host = new PeReader.DefaultHost();
                MyPlatformType myPlatformType = new MyPlatformType(host);

                string strAss, strType, strMethod;
                string strAssTarget, strTypeTarget, strMethodTarget;
                int i;
                string[] strAux;
                IMethodReference methodreference;
                IMethodDefinition methoddefinition;
                INamedTypeDefinition typemethod;
                IAssembly assemblymethod;

                IName ctor = host.NameTable.Ctor;

                //Referencias
                foreach (IAssemblyReference reference in assembly.AssemblyReferences)
                {
                    eFlowAssembly newEFlowAssembly = new eFlowAssembly();
                    newEFlowAssembly.Name = reference.Name.ToString();
                    newEFlowAssembly.Version = reference.ModuleIdentity.ContainingAssembly.Version.ToString();
                    newEFlowAssembly.Language = "C#.NET";
                    eFlowAssembly.Assemblies.Add(newEFlowAssembly);
                }

                //percorre os tipos do assembly
                foreach (INamedTypeDefinition type in assembly.GetAllTypes())
                {
                    if (type.Name.Value.Equals("<Module>"))
                        continue;
                    eFlowType eFlowType = new eFlowType();
                    eFlowType.Name = type.Name.Value;
                    eFlowType.FullName = type.ToString();
                    eFlowType.Kind = KindType(type);

                    //percorre os metodos
                    foreach (IMethodDefinition method in type.Methods)
                    {
                        eFlowMethod eFlowMethod = new eFlowMethod();
                        eFlowMethod.Name = method.Name.Value;
                        eFlowMethod.FullName = MemberHelper.GetMethodSignature(method, NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.ReturnType | NameFormattingOptions.Signature);
                        eFlowMethod.Visibility = method.Visibility.ToString();

                        // Alterado para os contrutores serem contabilizados
                        if (method.IsSpecialName && !method.IsConstructor)
                            continue;

                        // Se o metodo nao foi implementado para a CLR
                        if (!method.IsCil)
                            continue;

                        List<eFlowMethodException> eFlowMethodExceptions = new List<eFlowMethodException>();

                        GenerateExceptionInformation(method, myPlatformType, ref eFlowMethod, ref eFlowAssembly);

                        // Percorre as instruções do metodo PARA INSERIR AS CHAMADAS - CALL GRAPH
                        //foreach (IOperation ope in method.Body.Operations)
                        //{
                        //    //  SE A INSTRUÇÃO FOR UMA CHAMADA A UM MÉTODO
                        //    if (ope.OperationCode == OperationCode.Call || ope.OperationCode == OperationCode.Callvirt)
                        //    {
                        //        strAssTarget = ""; strTypeTarget = ""; strMethodTarget = "";

                        //        //recupera a referencia a chamada do metodo
                        //        methodreference = ope.Value as IMethodReference;
                        //        //recupera a definicao do metodo que foi chamado
                        //        methoddefinition = methodreference.ResolvedMethod;
                        //        if (methoddefinition == null)
                        //            continue;

                        //        //recupera o assembly de origem do metodo que foi chamado
                        //        assemblymethod = TypeHelper.GetDefiningUnit(methoddefinition.ContainingTypeDefinition) as IAssembly;
                        //        if (assemblymethod == null)
                        //            continue;
                        //        //recupera o type de origem do metodo que foi chamado
                        //        typemethod = methoddefinition.ContainingTypeDefinition as INamedTypeDefinition;
                        //        if (typemethod == null)
                        //            continue;

                        //        //recupera ou insere no banco o assembly de origem do metodo que foi chamado
                        //        strAssTarget = SqlHelper.ExecuteScalar(connection, "stpAssemblyIns", assemblymethod.ModuleName.Value, assemblymethod.ToString(), assemblymethod.Version.ToString(), null, assemblymethod.Kind.ToString(), strLinguagem, 0, "").ToString();
                        //        //recupera ou insere no banco o type de origem do metodo que foi chamado
                        //        strTypeTarget = SqlHelper.ExecuteScalar(connection, "stpTypeIns", strAssTarget, typemethod.Name.Value, typemethod.ToString(), KindType(typemethod)).ToString();
                        //        //recupera ou insere no banco o metodo que foi chamado
                        //        strMethodTarget = SqlHelper.ExecuteScalar(connection, "stpMethodIns", strTypeTarget, methoddefinition.Name.Value, MemberHelper.GetMethodSignature(methoddefinition, NameFormattingOptions.OmitContainingNamespace | NameFormattingOptions.ReturnType | NameFormattingOptions.Signature), methoddefinition.Visibility.ToString()).ToString();
                        //        //recupera a informações sobre EH do metodo, quais exceptions etc
                        //        arrayException.Clear();
                        //        arrayException = GenerateExceptionInformation(methoddefinition, myPlatformType);
                        //        //insere as informações sobre EH
                        //        for (int j = 0; j < arrayException.Count; j++)
                        //        {
                        //            strAux = arrayException[j].ToString().Split('|');
                        //            //SqlHelper.ExecuteNonQuery(connection, "stpMethodExceptionIns", strMethodTarget, strAux[0], strAux[1], strAux[2], strAux[3], strAux[4], strAux[5]);
                        //        }
                        //        //atualiza a tabela de methos com os totalizadors de EH
                        //        SqlHelper.ExecuteNonQuery(connection, "stpMethodUpdException", strMethodTarget);

                        //        //se o assembly da chamada do metodo for diferente do assembly que esta sendo analisado
                        //        if !(TypeHelper.NamespaceTypesAreEquivalent(met.ContainingType,mett.ContainingType))
                        //                if (ass.ModuleName.Value != asss.ModuleNazme.Value)
                        //               continue;                              

                        //        i += 1;
                        //        SqlHelper.ExecuteNonQuery(connection, "stpMethodCallIns", strAss, strMethod, strMethodTarget, i, ope.Offset);
                        //    }
                        //}

                        eFlowType.Methods.Add(eFlowMethod);
                    }

                    eFlowAssembly.Types.Add(eFlowType);
                }

                //  PROCESSA O FLUXO EXCEPCIONAL
                //ProcessCCIExceptionFlow(int.Parse(strAss));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eFlowAssembly"></param>
        /// <returns></returns>
        private static string XmlSerializeObject(eFlowAssembly eFlowAssembly)
        {
            var stringwriter = new System.IO.StringWriter();
            var serializer = new XmlSerializer(typeof(eFlowAssembly));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces(); ns.Add("", "");
            serializer.Serialize(stringwriter, eFlowAssembly, ns);
            string xml = stringwriter.ToString();
            return xml;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        private static string DataContractSerializeObject<T>(T objectToSerialize)
        {
            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) { Formatting = System.Xml.Formatting.Indented })
            {
                //new DataContractSerializer(typeof(T),
                //    new DataContractSerializerSettings
                //    {
                //        PreserveObjectReferences = true
                //    })

                new DataContractSerializer(typeof(T), null,
                0x7FFF /*maxItemsInObjectGraph*/,
                false /*ignoreExtensionDataObject*/,
                true /*preserveObjectReferences*/,
                null /*dataContractSurrogate*/).WriteObject(writer, objectToSerialize);
                return output.GetStringBuilder().ToString();
            }

            //using (MemoryStream memStm = new MemoryStream())
            //{
            //    var serializer = new DataContractSerializer(typeof(T), null,
            //    0x7FFF /*maxItemsInObjectGraph*/,
            //    false /*ignoreExtensionDataObject*/,
            //    true /*preserveObjectReferences*/,
            //    null /*dataContractSurrogate*/);

            //    serializer.WriteObject(memStm, objectToSerialize);

            //    memStm.Seek(0, SeekOrigin.Begin);

            //    using (var streamReader = new StreamReader(memStm))
            //    {
            //        string result = streamReader.ReadToEnd();
            //        return result;
            //    }
            //}
        }

        /// <summary>
        /// Captura as chamadas de métodos dentros dos Try/Catch/Finally
        /// </summary>
        /// <param name="eFlowAssembly"></param>
        private static void GenerateMethodCalls(eFlowAssembly eFlowAssembly)
        {
            foreach (var type in eFlowAssembly.Types)
            {
                foreach (var method in type.Methods)
                {
                    int opIndex = 0;
                    foreach (var op in method.MethodDefinition.Body.Operations)
                    {
                        opIndex++;
                        if (op.OperationCode == OperationCode.Call || op.OperationCode == OperationCode.Callvirt)
                        {
                            if (!method.MethodDefinition.IsConstructor)
                            {
                                IMethodReference MethodReference = op.Value as IMethodReference;
                                IMethodDefinition MethodDefinition = MethodReference.ResolvedMethod;
                                INamedTypeDefinition NamedTypeDefinition = MethodDefinition.ContainingTypeDefinition as INamedTypeDefinition;
                                
                                foreach (var item in eFlowAssembly.Types)
                                {
                                    eFlowMethod eFlowMethodReference = new eFlowMethod();
                                    eFlowMethodReference = item.Methods.Find(m => m.FullName.Equals(op.Value.ToString()));

                                    if (eFlowMethodReference != null)
                                    {
                                        eFlowMethodCall eFlowMethodCall = new eFlowMethodCall();
                                        eFlowMethodCall.MethodSource = method;
                                        eFlowMethodCall.MethodTarget = eFlowMethodReference;
                                        eFlowMethodCall.OffSet = op.Offset.ToString();
                                        eFlowMethodCall.Order = opIndex.ToString();
                                        eFlowAssembly.MethodCalls.Add(eFlowMethodCall);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //if (!method.MethodDefinition.IsAbstract && !method.MethodDefinition.IsExternal)
                    //{
                    //    IMetadataReaderHost metadataHost = new Microsoft.Cci.PeReader.DefaultHost();
                    //    ISourceMethodBody sourceMethodBody = Decompiler.GetCodeModelFromMetadataModel(metadataHost, method.MethodDefinition.Body, null);
                    //    List<IStatement> TryCatchFinallyStatements = sourceMethodBody.Block.Statements.ToList().FindAll
                    //            (st => st.GetType().Equals(typeof(Microsoft.Cci.MutableCodeModel.TryCatchFinallyStatement)));

                    //    if (TryCatchFinallyStatements.Count > 0)
                    //    {
                    //        //TODO: ...
                    //        // Percorre o bloco Finally em busca de chamadas de métodos
                    //        if (((ITryCatchFinallyStatement)TryCatchFinallyStatements[0]).FinallyBody != null)
                    //        {
                    //            // TODO: verificar melhor este caso de quando há um finally ter de descobrir o novo bloco
                    //            // Redescobrir um novo bloco TryCatchFinallyStatement quando temos um finally na classe

                    //            // TODO: fazer do outro jeito - compativel
                    //            ITryCatchFinallyStatement TryBlockTest = TryCatchFinallyStatements[0] as ITryCatchFinallyStatement;
                    //            if (TryBlockTest == null)
                    //                TryCatchFinallyStatements[0] = ((ITryCatchFinallyStatement)TryCatchFinallyStatements[0]).TryBody.Statements.First();


                    //        }

                    //        //TODO: Verificar se tem como ter mais de um statement Try
                    //        // Percorre o bloco Try em busca de chamadas de métodos
                    //        List<IStatement> statements = ((ITryCatchFinallyStatement)TryCatchFinallyStatements[0]).TryBody.Statements.ToList().FindAll
                    //                (st => st.GetType().Equals(typeof(IExpressionStatement)) &&
                    //                 ((IExpressionStatement)st).Expression.GetType().Equals(typeof(IMethodCall)));

                    //        foreach (IExpressionStatement statement in statements)
                    //        {
                    //            eFlowMethodCall eFlowMethodCall = new eFlowMethodCall();

                    //            eFlowAttributeReference MethodSourceReference = new eFlowAttributeReference();
                    //            MethodSourceReference.Reference = 0;

                    //            eFlowAttributeReference MethodTargetReference = new eFlowAttributeReference();
                    //            foreach (var newType in eFlowAssembly.Types)
                    //                foreach (var newMethods in newType.Methods)
                    //                    if (newMethods.FullName.Equals(((IMethodCall)statement.Expression).MethodToCall.ResolvedMethod.ToString()))
                    //                        MethodTargetReference.Reference = 1;

                    //            //eFlowMethodCall.MethodSourceReference = MethodSourceReference;
                    //            //eFlowMethodCall.MethodTargetReference = MethodTargetReference;
                    //            //eFlowMethodCall.OffSet = ((ITryCatchFinallyStatement)TryCatchFinallyStatements[0]).TryBody.Statements.ToList().IndexOf(statement);
                    //            //eFlowMethodCall.Order = statements.IndexOf(statement);
                    //            eFlowAssembly.MethodCalls.Add(eFlowMethodCall);
                    //        }

                    //        //TODO: ...
                    //        // Percorre o bloco Catch em busca de chamadas de métodos


                    //    }
                    //}
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static void GenerateExceptionInformation(IMethodDefinition m, MyPlatformType p, ref eFlowMethod eFlowMethod, ref eFlowAssembly eFlowAssembly)
        {
            bool lastInstructionWasNewObj = false;
            IMethodReference consRef = null;
            eFlowMethodException eFlowMethodException;

            // Procura as exceções das instruções Catch
            foreach (IOperationExceptionInformation OperationException in m.Body.OperationExceptionInformation)
            {
                switch (OperationException.HandlerKind)
                {
                    case HandlerKind.Catch:
                        eFlowMethodException = new eFlowMethodException();
                        //eFlowMethodException.ExceptionReference = new eFlowException(Type.GetType(OperationException.ExceptionType.ResolvedType.ToString()));
                        eFlowMethodException.StartOffSet = OperationException.HandlerStartOffset.ToString();
                        eFlowMethodException.EndOffSet = OperationException.HandlerEndOffset.ToString();
                        eFlowMethodException.Kind = OperationException.HandlerKind.ToString();
                        eFlowMethodException.IsGeneric = IsGeneric(OperationException.ExceptionType);
                        eFlowMethod.MethodExceptions.Add(eFlowMethodException);
                        break;
                    case HandlerKind.Finally:
                        eFlowMethodException = new eFlowMethodException();
                        //TODO: Identificar o tipo do Throw do bloco Finally
                        //eFlowMethodException.ExceptionReference = new eFlowException(Type.GetType(ope.ExceptionType.ResolvedType.ToString()));
                        eFlowMethodException.StartOffSet = OperationException.TryStartOffset.ToString();
                        eFlowMethodException.EndOffSet = OperationException.TryEndOffset.ToString();
                        eFlowMethodException.Kind = OperationException.HandlerKind.ToString();
                        eFlowMethodException.IsGeneric = IsGeneric(OperationException.ExceptionType);
                        eFlowMethod.MethodExceptions.Add(eFlowMethodException);
                        break;
                    default:
                        throw new Exception("Tratar HandlerKind");
                }

                // Registra a Exception no assembly
                if (OperationException.HandlerKind != HandlerKind.Finally)
                    if (eFlowAssembly.Exceptions.FindAll(e => e.Name.Equals(Type.GetType(OperationException.ExceptionType.ResolvedType.ToString()))).Count == 0)
                        eFlowAssembly.Exceptions.Add(new eFlowException(Type.GetType(OperationException.ExceptionType.ResolvedType.ToString())));
            }

            //TODO: Identificar o bloco de cada Throw
            // Procura as instruções Throw em todos os blocos
            foreach (IOperation Operation in m.Body.Operations)
            {
                if (Operation.OperationCode == OperationCode.Newobj)
                {
                    consRef = Operation.Value as IMethodReference;
                    lastInstructionWasNewObj = true;
                }
                else if (lastInstructionWasNewObj && Operation.OperationCode == OperationCode.Throw)
                {
                    eFlowMethodException = new eFlowMethodException();
                    //eFlowMethodException.ExceptionReference = new eFlowException(Type.GetType(consRef.ContainingType.ResolvedType.ToString()));
                    eFlowMethodException.StartOffSet = Operation.Offset.ToString();
                    eFlowMethodException.EndOffSet = Operation.Offset.ToString();
                    eFlowMethodException.Kind = "Throw";
                    eFlowMethodException.IsGeneric = IsGeneric(consRef.ContainingType);
                    eFlowMethod.MethodExceptions.Add(eFlowMethodException);

                    lastInstructionWasNewObj = false;

                    if (eFlowAssembly.Exceptions.FindAll(e => e.Name.Equals(Type.GetType(consRef.ContainingType.ResolvedType.ToString()))).Count == 0)
                        eFlowAssembly.Exceptions.Add(new eFlowException(Type.GetType(consRef.ContainingType.ResolvedType.ToString())));

                    eFlowMethod.QtdThrow++;
                }
                else if (!lastInstructionWasNewObj && Operation.OperationCode == OperationCode.Throw)
                {
                    eFlowMethodException = new eFlowMethodException();
                    //eFlowMethodException.ExceptionReference = new eFlowException(Type.GetType(consRef.ContainingType.ResolvedType.ToString()));
                    eFlowMethodException.StartOffSet = Operation.Offset.ToString();
                    eFlowMethodException.EndOffSet = Operation.Offset.ToString();
                    eFlowMethodException.Kind = "Re-Throw";
                    eFlowMethodException.IsGeneric = IsGeneric(consRef.ContainingType);
                    eFlowMethod.MethodExceptions.Add(eFlowMethodException);

                    lastInstructionWasNewObj = false;

                    if (eFlowAssembly.Exceptions.FindAll(e => e.Name.Equals(Type.GetType(consRef.ContainingType.ResolvedType.ToString()))).Count == 0)
                        eFlowAssembly.Exceptions.Add(new eFlowException(Type.GetType(consRef.ContainingType.ResolvedType.ToString())));

                    eFlowMethod.QtdThrow++;
                }
                else
                {
                    lastInstructionWasNewObj = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal class MyPlatformType : PlatformType
        {
            INamespaceTypeReference systemException;
            INamespaceTypeReference exception;
            INamespaceTypeReference applicationException;

            internal MyPlatformType(IMetadataHost host)
                : base(host)
            {
            }

            public INamespaceTypeReference SystemException
            {
                get
                {
                    if (this.systemException == null)
                    {
                        this.systemException = this.CreateReference(this.CoreAssemblyRef, "System", "SystemException");
                    }
                    return this.systemException;
                }
            }

            public INamespaceTypeReference Exception
            {
                get
                {
                    if (this.exception == null)
                    {
                        this.exception = this.CreateReference(this.CoreAssemblyRef, "System", "Exception");
                    }
                    return this.exception;
                }
            }

            public INamespaceTypeReference ApplicationException
            {
                get
                {
                    if (this.applicationException == null)
                    {
                        this.applicationException = this.CreateReference(this.CoreAssemblyRef, "System", "ApplicationException");
                    }
                    return this.applicationException;
                }
            }
        }
    }
}
