using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        //Setup
        public void TestMethod()
        {
            //String command = @"C:\Doit.bat";
            //ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);

            //ExecuteILRepackMerge();

            Assembly assembly;
            string newAssemblyPath;
            string assemblyPath;

            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
            assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");

            newAssemblyPath = WeaverHelper.Weave(assemblyPath);

            assembly = Assembly.LoadFile(newAssemblyPath);
        }

        [TestMethod]
        public void ExecuteILRepackMerge()
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "ILRepack.dll /out:Sample_merge.exe Sample_aop.exe Mono.Cecil.dll DotNetFlow.exe");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            process.Close();
        }
    }
}
