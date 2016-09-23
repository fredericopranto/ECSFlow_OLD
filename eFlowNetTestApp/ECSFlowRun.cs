using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ECSFlowRun
{
    class ECSFlowRun
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Weaving starting...");


            //String command = @"C:\Doit.bat";
            //ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            //ExecuteILRepackMerge();

            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcessFlow\AssemblyToProcessFlow.csproj"));
            var assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcessFlow.exe");

            //var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Ascgen2NoTry\Ascgen2.csproj"));
            //var assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"Ascgen2\bin\Debug\Ascgen2.exe");

            var newAssemblyPath = WeaverHelper.Weave(assemblyPath);

            Verifier.Verify(assemblyPath, newAssemblyPath);
            Console.WriteLine("Weaving verified");

            var assembly = Assembly.LoadFile(newAssemblyPath);
            Console.WriteLine("New assembly created");
            Console.Read();
        }

        static void ExecuteILRepackMerge()
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
