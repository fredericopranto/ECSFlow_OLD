using System.IO;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;

namespace ECSFlow.Helper
{
    public static class WeaverHelper
    {

        public static string Weave(string assemblyPath)
        {
            string newAssemblyPath, newAssemblyPDBPath;
            GenerateNewAssembly(assemblyPath, out newAssemblyPath, out newAssemblyPDBPath);

            using (var symbolStream = File.OpenRead(newAssemblyPDBPath))
            {
                //Read new assembly
                var readerParameters = new ReaderParameters
                {
                    ReadSymbols = true,
                    SymbolStream = symbolStream,
                    SymbolReaderProvider = new PdbReaderProvider()
                };
                var moduleDefinition = ModuleDefinition.ReadModule(newAssemblyPath, readerParameters);

                //Weaving configuration
                var weavingTask = new ModuleWeaver
                {
                    ModuleDefinition = moduleDefinition,
                    AssemblyResolver = new DefaultAssemblyResolver(),
                    AssemblyPath = assemblyPath
                };

                //Weaving process
                weavingTask.Execute();

                Verifier.Verify(assemblyPath, newAssemblyPath);
                Console.WriteLine("Weaving verified");

                //Write new assembly modified
                moduleDefinition.Write(newAssemblyPath);
                return newAssemblyPath;
            }
        }

        private static void GenerateNewAssembly(string assemblyPath, out string newAssemblyPath, out string newAssemblyPDBPath)
        {
            var extension = Path.GetExtension(assemblyPath);
            newAssemblyPath = assemblyPath.Replace(extension, string.Concat("2", extension));
            var oldPdb = assemblyPath.Replace(extension, ".pdb");
            newAssemblyPDBPath = assemblyPath.Replace(extension, "2.pdb");
            File.Copy(assemblyPath, newAssemblyPath, true);
            File.Copy(oldPdb, newAssemblyPDBPath, true);
        }
    }
}