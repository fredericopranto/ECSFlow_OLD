using ECSFlow.Fody;

// Information about global exception handling specification

#region View (Forms)

[assembly: ExceptionRaiseSite("rSite1", "FormConvertImage.LoadImage")]
[assembly: ExceptionChannel("EEC1", new string[] { "OutOfMemoryException" }, new string[] { "rSite1" } ) ]
[assembly: ExceptionChannel("EEC2", new string[] { "FileNotFoundException" }, new string[] { "rSite1" })]

#endregion

#region MainHandler

[assembly: ExceptionHandler(new string[] { "EEC1", "EEC2" }, "Ascgen2.Main", 
        new string[] { "OutOfMemoryException" }, 
        typeof(Ascgen2ControllerHandler), nameof(Ascgen2ControllerHandler.OutOfMemoryExceptionHandler))]

[assembly: ExceptionHandler(new string[] { "EEC1", "EEC2" }, "Ascgen2.Main",
        new string[] { "FileNotFoundException" },
        typeof(Ascgen2ControllerHandler), nameof(Ascgen2ControllerHandler.FileNotFoundExceptionHandler))]



// General handler implementation
public struct Ascgen2ControllerHandler
{
    public static void OutOfMemoryExceptionHandler(System.OutOfMemoryException e)
    {
        System.Console.WriteLine("OutOfMemoryException caught");
        System.Console.WriteLine(e.Message);
    }

    public static void FileNotFoundExceptionHandler(System.IO.FileNotFoundException e)
    {
        System.Console.WriteLine("FileNotFoundException caught");
        System.Console.WriteLine(e.Message);
    }
}

#endregion