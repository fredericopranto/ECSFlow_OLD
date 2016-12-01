using ECSFlowAttributes;
using System;

namespace ECSFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            SomeMethod();
        }

        [MethodLogging]
        public static void SomeMethod()
        {
            Console.WriteLine("SomeMethod Body");
            Console.ReadLine();
        }
    }
}
