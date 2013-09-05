using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FA.Tests
{
    class NFATest
    {
        public static void SimpleTest()
        {
            String[] regexps = { "a(d|c)+a", "a*((c+|d+)*)?", "a*(c*|d*)*?"};
            String[] strings = { "aaaaaaaa", "adad", "aaaadcddaaaa", "adcdcdcdca", "cddc", "aacddc", "cccccc", "dddddd" };

            foreach (var re in regexps)
            {
                Console.WriteLine("Regex: " + re);
                Console.WriteLine();
                NFA nfa = NFA.FromRe(re);
                foreach (var str in strings)
                {
                    Console.WriteLine(str + " - " + nfa.Match(str) + ". Expected: " + new Regex('^' + re + '$').IsMatch(str).ToString());
                }
                Console.WriteLine("\r\n//-------------------\r\n");
            }
        }
        /// <summary>
        /// Будем матчить a^n по регулярке a?^na^n.
        /// Где x^n - просто n раз повторение регекса или символа x;
        /// </summary>
        /// <param name="n"></param>
        public static void PerformanceTest(int n)
        {
            Console.WriteLine("Performance test. n = " + n.ToString());

            StringBuilder re = new StringBuilder();
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                re.Append("a?");
            }
            for (int i = 0; i < n; i++)
            {
                re.Append("a");
                str.Append("a");
            }

            bool nfaMatch, CSharpMatch;
            TimeSpan nfaTime, CSharpTime;
            TimeSpan nfaModelingTime, CSharpModelingTime;

            Stopwatch sw = new Stopwatch();

            Console.WriteLine("Regular expression: " + re);
            Console.WriteLine("Matching string: " + str);

            sw.Start();
            NFA nfa = NFA.FromRe(re.ToString());
            sw.Stop();
            nfaModelingTime = sw.Elapsed;
            sw.Reset();

            sw.Start();
            nfaMatch = nfa.Match(str.ToString());
            sw.Stop();
            nfaTime = sw.Elapsed;
            sw.Reset();

            Console.WriteLine(
                "Thompson's NFA algorithm:\r\nResult: " + nfaMatch.ToString() +
                ". Modeling time: " + nfaModelingTime.TotalSeconds + "seconds" +
                ". Match time: " + nfaTime.TotalSeconds + " seconds.");

            sw.Start();
            Regex regex = new Regex(re.ToString());
            sw.Stop();
            CSharpModelingTime = sw.Elapsed;
            sw.Reset();

            sw.Start();
            CSharpMatch = regex.IsMatch(str.ToString());
            sw.Stop();
            CSharpTime = sw.Elapsed;
            sw.Reset();


            Console.WriteLine(
                "C# Regex class:\r\nResult: " + CSharpMatch.ToString() +
                ". Modeling time: " + CSharpModelingTime.TotalSeconds + "seconds" +
                ". Match time: " + CSharpTime.TotalSeconds + " seconds.\r\n\r\n");
        }
    }
}
