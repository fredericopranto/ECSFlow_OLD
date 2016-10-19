using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ECSFlow;

[TestClass]
public class WeaverTests
{
    Assembly assembly;
    string newAssemblyPath;
    string assemblyPath;

    [TestInitialize]
    public void Setup()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Ascgen2NoTry\Ascgen2.csproj"));
        assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"Ascgen2\bin\Debug\Ascgen2.exe");

        newAssemblyPath = assemblyPath.Replace(".exe", "2.exe");
        File.Copy(assemblyPath, newAssemblyPath, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssemblyPath);
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition
        };

        weavingTask.Execute();
        moduleDefinition.Write(newAssemblyPath);

        assembly = Assembly.LoadFile(newAssemblyPath);
    }

    [TestMethod]
    public void eFlowValidateHelloWorldIsInjected()
    {
        var type = assembly.GetType("Hello");
        var instance = (dynamic) Activator.CreateInstance(type);

        Assert.AreEqual("Hello World", instance.World());
    }

#if(DEBUG)
   // [TestMethod]
    public void eFlowPeVerify()
    {
        Verifier.Verify(assemblyPath,newAssemblyPath);
    }
#endif
}