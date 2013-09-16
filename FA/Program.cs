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
            try
            {
                Tests.NFATest.SimpleTest();
            }
            catch (InvalidRegexException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            //Tests.NFATest.PerformanceTest(25);
        }
    }
}
