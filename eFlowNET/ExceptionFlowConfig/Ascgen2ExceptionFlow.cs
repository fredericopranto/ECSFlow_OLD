using eFlowNET.Fody;
using System;
using System.Data.Common;
using System.Data.SqlClient;

// General information about global exception handling specification

#region Util Injection

[assembly: ExceptionRaiseSite("rSite2", "getImageInfoFromBytes()")]
[assembly: ExceptionChannel("EEC2", new string[] { "IndexOutOfRangeException", "ArrayStoreException" }, false, "rSite2")]
[assembly: ExceptionInterface("EEC2", "lancs.mobilemedia.core.util", true, "LibException")]

#endregion

#region Model Injection

[assembly: ExceptionRaiseSite("rSite1", "lancs.mobilemedia.core.ui.datamodel.*")]
[assembly: ExceptionChannel("EEC1", new string[] { "RecordStoreException"}, true, "rSite1")]
[assembly: ExceptionInterface("EEC1", "rSite1", false)]

[assembly: ExceptionRaiseSite("EEC2", "Util.EEC2")]
[assembly: ExceptionInterface("EEC2", "rSite1", false)]

#endregion

#region Control Injection

[assembly: ExceptionRaiseSite("EEC1", "Model.EEC1")]
[assembly: ExceptionRaiseSite("EEC2", "Model.EEC2")]
[assembly: ExceptionHandler(new string[] { "EEC2", "EEC1" }, "BaseController", 
    typeof(BaseControllerHandler), nameof(BaseControllerHandler.handler))]

#endregion

#region View Injection

[assembly: ExceptionRaiseSite("EEC1", "Model.EEC1")]
[assembly: ExceptionHandler("EEC1", "lancs.mobilemedia.core.ui.screens.*", 
    typeof(BaseControllerHandler), nameof(BaseControllerHandler.handler))]

#endregion

// General handler implementation
public struct BaseControllerHandler
{
    public static void handler(Exception e)
    { Console.WriteLine(e.Message); }
}


/// <summary>
/// General Information about global exception handling implementation
/// </summary>
public static class GlobalExceptionImpl
{
    /// <summary>
    /// Type Handler: Exception
    /// </summary>
    /// <param name="ex"></param>
    public static void ExceptionTypeHandler(Exception ex)
    {
        Console.WriteLine(">> Type Handler: Exception");
    }

    /// <summary>
    /// Type Handler: SqlException
    /// </summary>
    /// <param name="sqlEx"></param>
    public static void SqlExceptionTypeHandler(SqlException sqlEx)
    {
        Console.WriteLine(">> Type Handler: SqlException");
    }

    /// <summary>
    /// Type Handler: DbException
    /// </summary>
    /// <param name="ex"></param>
    public static void DbExceptionTypeHandler(DbException ex)
    {
        Console.WriteLine(">> Type Handler: DbException");
    }
}
