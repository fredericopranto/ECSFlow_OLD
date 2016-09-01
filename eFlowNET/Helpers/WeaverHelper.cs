using System.IO;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using ECSFlow.Fody;

public static class WeaverHelper
{

    public static string Weave(string assemblyPath)
    {
        string newAssembly, newAssemblyPDB;
        GenerateNewAssembly(assemblyPath, out newAssembly, out newAssemblyPDB);

        using (var symbolStream = File.OpenRead(newAssemblyPDB))
        {
            //Read new assembly
            var readerParameters = new ReaderParameters
            {
                ReadSymbols = true,
                SymbolStream = symbolStream,
                SymbolReaderProvider = new PdbReaderProvider()
            };
            var moduleDefinition = ModuleDefinition.ReadModule(newAssembly, readerParameters);

            //Weaving configuration
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
                AssemblyResolver = new DefaultAssemblyResolver()
            };
            
            //Weaving process
            weavingTask.Execute();

            //Write new assembly modified
            moduleDefinition.Write(newAssembly);
            return newAssembly;
        }
    }

    private static void GenerateNewAssembly(string assemblyPath, out string newAssembly, out string newAssemblyPDB)
    {
        var extension = Path.GetExtension(assemblyPath);
        newAssembly = assemblyPath.Replace(extension, string.Concat("2", extension));
        var oldPdb = assemblyPath.Replace(extension, ".pdb");
        newAssemblyPDB = assemblyPath.Replace(extension, "2.pdb");
        File.Copy(assemblyPath, newAssembly, true);
        File.Copy(oldPdb, newAssemblyPDB, true);
    }
}