using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    class Program
    {
        static void Main(string[] args)
        {
            FA.Tests.NFATest.SimpleTest();
            Tests.NFATest.PerformanceTest(25);
        }
    }
}
