using System.IO;
using Mono.Cecil;
using Mono.Cecil.Pdb;

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