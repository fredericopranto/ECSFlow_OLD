using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab
{
    class MyClass
    {
        int limit = 0;
        public MyClass(int limit) { this.limit = limit; }

        public IEnumerable<int> CountFrom(int start)
        {
            for (int i = start; i <= limit; i++)
            {
                yield return i;
            }
        }

        static void Main(string[] args)
        {
            MyClass m = new MyClass(10);
            Console.WriteLine(m.CountFrom(0).ToString());
            Console.Read();
        }
    }
}
