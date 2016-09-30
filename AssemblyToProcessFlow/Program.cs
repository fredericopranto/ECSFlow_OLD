using System;

namespace AssemblyToProcessFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            SomeMethod();
        }

        //[ExceptionRaiseSite("rSite1", "Program.SomeMethod")]
        //[ExceptionChannel("EEC1", new string[] { "System.OutOfMemoryException" }, new string[] { "rSite1" })]
        public static void SomeMethod()
        {
            throw new OutOfMemoryException();
        }
    }
}
