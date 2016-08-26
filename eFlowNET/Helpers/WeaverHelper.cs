using System.IO;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using eFlowNET.Fody;

public static class WeaverHelper
{

    public static string Weave(string assemblyPath)
    {
        var extension = Path.GetExtension(assemblyPath);

        var newAssembly = assemblyPath.Replace(extension, string.Concat("2", extension));
        var oldPdb = assemblyPath.Replace(extension, ".pdb");
        var newPdb = assemblyPath.Replace(extension, "2.pdb");
        File.Copy(assemblyPath, newAssembly, true);
        File.Copy(oldPdb, newPdb, true);

        //var assemblyResolver = new MockAssemblyResolver
        //{
        //    Directory = Path.GetDirectoryName(assemblyPath)
        //};

        using (var symbolStream = File.OpenRead(newPdb))
        {
            var readerParameters = new ReaderParameters
            {
                ReadSymbols = true,
                SymbolStream = symbolStream,
                SymbolReaderProvider = new PdbReaderProvider()
            };
            var moduleDefinition = ModuleDefinition.ReadModule(newAssembly, readerParameters);


            //ModuleDefinition eFlowModule = ModuleDefinition.ReadModule("ECSFlowNET.dll");

            //// Code to deep copy the reference assembly into the main assembly
            //var importer = new TypeImporter(eFlowModule, moduleDefinition.Assembly.MainModule);
            //foreach (var definition in moduleDefinition.Assembly.Modules.SelectMany(x => x.Types).ToArray())
            //{
            //    importer.Import(definition);
            //}

            //var exceptionType = system.MainModule.GetTypes().First(x => x.Name == exception.Name);
            //moduleDefinition.ImportReference(exceptionType);


            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new DefaultAssemblyResolver()
            };

            // Weaving process
            weavingTask.Execute();

            // Reassembly process
            moduleDefinition.Write(newAssembly);

            return newAssembly;
        }
    }
}