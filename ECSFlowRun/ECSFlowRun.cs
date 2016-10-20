using ECSFlow.Helper;
using System;
using System.Configuration;
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

            var apps = ConfigurationManager.AppSettings["target"].ToString().Split(';');

            foreach (var app in apps)
            {
                var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, string.Format(@"..\..\..\{0}\{0}.csproj", app)));
                var assemblyPath = Path.GetDirectoryName(projectPath) + string.Format(@"\bin\Debug\{0}.exe", app);
                var assemblyPathNoTry = Path.GetDirectoryName(projectPath) + string.Format(@"\bin\Debug\{0}2.exe", app);

                var newAssemblyPath = WeaverHelper.Weave(assemblyPath);

                Verifier.Verify(assemblyPath, newAssemblyPath);
                Console.WriteLine("Weaving verified");

                var assembly = Assembly.LoadFile(newAssemblyPath);
                Console.WriteLine("New assembly created");
            }

            Console.WriteLine("Process completed.");
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