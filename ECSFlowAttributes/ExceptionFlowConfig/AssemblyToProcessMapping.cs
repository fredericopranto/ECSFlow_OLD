using ECSFlow.Fody;
using System.Windows.Forms;

[assembly: ExceptionRaiseSite(typeof(AssemblyToProcessMapping), "rSite1", "Program.Main")]
[assembly: ExceptionChannel(typeof(AssemblyToProcessMapping), "EEC1", new string[] { "System.OutOfMemoryException" }, new string[] { "rSite1" })]
[assembly: ExceptionChannel(typeof(AssemblyToProcessMapping), "EEC2", new string[] { "System.IO.FileNotFoundException" }, new string[] { "rSite1" })]
[assembly: ExceptionHandler(new string[] { "EEC1", "EEC2" }, "Program.Main",
        new string[] { "System.OutOfMemoryException" },
        typeof(AssemblyToProcessMapping), nameof(AssemblyToProcessMapping.OutOfMemoryExceptionHandler))]


public struct AssemblyToProcessMapping
{
    public static void OutOfMemoryExceptionHandler(System.OutOfMemoryException e)
    {
        MessageBox.Show("OutOfMemoryException caught");
        MessageBox.Show(e.Message);
        
    }
}