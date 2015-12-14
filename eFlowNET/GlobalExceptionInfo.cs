using eFlowNET.Fody;
using System;

// General information about global exception handling specification

#region Util Injection

[assembly: ExceptionRaiseSite("rSite2", "getImageInfoFromBytes()")]
[assembly: ExceptionChannel("EEC2", new string[] { "IndexOutOfRangeException", "ArrayStoreException" }, false, "rSite2")]
[assembly: ExceptionInterface("EEC2", "lancs.mobilemedia.core.util", true, typeof(LibException))]

#endregion

#region Model Injection

[assembly: ExceptionRaiseSite("rSite1", "lancs.mobilemedia.core.ui.datamodel.*", RaiseSiteScope.Namespace)]
[assembly: ExceptionChannel("EEC1", new string[] { "RecordStoreException"}, true, "rSite1")]
[assembly: ExceptionInterface("EEC1", "rSite1", false)]

[assembly: ExceptionRaiseSite("EEC2", "Util.EEC2", RaiseSiteScope.ByPass)]
[assembly: ExceptionInterface("EEC2", "rSite1", false)]

#endregion

#region Control Injection

[assembly: ExceptionRaiseSite("EEC1", "Model.EEC1", RaiseSiteScope.ByPass)]
[assembly: ExceptionRaiseSite("EEC2", "Model.EEC2", RaiseSiteScope.ByPass)]
[assembly: ExceptionHandler(new string[] { "EEC2", "EEC1" }, "BaseController")]

#endregion

#region View Injection

[assembly: ExceptionRaiseSite("EEC1", "Model.EEC1", RaiseSiteScope.ByPass)]
[assembly: ExceptionHandler("EEC1", "lancs.mobilemedia.core.ui.screens.*", RaiseSiteScope.Namespace)]

#endregion


// General information about defined Interface exception


#region Interface Exception List

class LibException : Exception { }

#endregion