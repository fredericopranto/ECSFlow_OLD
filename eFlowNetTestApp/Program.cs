using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly assembly;
            string newAssemblyPath;
            string assemblyPath;

            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
            assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");

            newAssemblyPath = WeaverHelper.Weave(assemblyPath);

            newAssemblyPath = WeaverHelper.Weave(assemblyPath);

            assembly = Assembly.LoadFile(newAssemblyPath);
        }
    }
}
