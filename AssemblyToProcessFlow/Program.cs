using ECSFlow.Fody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcessFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            SomeMethod();
        }

        [ExceptionRaiseSite("rSite1", "Program.SomeMethod")]
        [ExceptionChannel("EEC1", new string[] { "System.OutOfMemoryException" }, new string[] { "rSite1" })]
        public static void SomeMethod()
        {
            throw new OutOfMemoryException();
        }
    }
}
