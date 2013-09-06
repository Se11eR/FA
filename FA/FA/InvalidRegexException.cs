using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    public class InvalidRegexException: Exception
    {
        public int CharNum { get; private set; }

        public InvalidRegexException()
        {
        }

        public InvalidRegexException(string message)
            : base(message)
        {
        }

        public InvalidRegexException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidRegexException(string message, int char_num)
        {
            CharNum = char_num;
        }
    }
}
