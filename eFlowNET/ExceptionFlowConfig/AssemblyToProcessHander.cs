using ECSFlow.Fody;

// Information about global exception handling specification

#region View (Forms)

[assembly: ExceptionRaiseSite("rSite1", "FormConvertImage.LoadImage")]
[assembly: ExceptionChannel("EEC1", new string[] { "System.OutOfMemoryException" }, new string[] { "rSite1" } ) ]
[assembly: ExceptionChannel("EEC2", new string[] { "System.IO.FileNotFoundException" }, new string[] { "rSite1" })]

#endregion

#region MainHandler

[assembly: ExceptionHandler(new string[] { "EEC1", "EEC2" }, "Program.Main", 
        new string[] { "System.OutOfMemoryException" }, 
        typeof(AssemblyToProcessHander), nameof(AssemblyToProcessHander.OutOfMemoryExceptionHandler))]

public struct AssemblyToProcessHander
{
    public static void OutOfMemoryExceptionHandler(System.OutOfMemoryException e)
    {
        System.Console.WriteLine("OutOfMemoryException caught");
        System.Console.WriteLine(e.Message);
        System.Console.Read();
    }

    public static void FileNotFoundExceptionHandler(System.IO.FileNotFoundException e)
    {
        System.Console.WriteLine("FileNotFoundException caught");
        System.Console.WriteLine(e.Message);
        System.Console.Read();
    }
}

#endregion