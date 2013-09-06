using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    public enum Operation
    {
        Char,
        Match,
        Jmp,
        Split,
        Empty
    }

    public class Instruction
    {
        public static readonly Instruction MatchInst = new Instruction(Operation.Match);

        public readonly Operation OperationCode;
        public readonly int c;

        public Instruction next;
        public Instruction split1;
        public Instruction split2;

        private Instruction(Operation o)
        {
            OperationCode = o;
        }

        public Instruction()
        {
            OperationCode = Operation.Empty;
            c = 0;
            split1 = null;
            split2 = null;
            next = null;
        }

        public Instruction(int ch)
        {
            OperationCode = Operation.Char;
            c = ch;
            split1 = null;
            split2 = null;
            next = null;
        }

        public Instruction(Instruction i1, Instruction i2)
        {
            OperationCode = Operation.Split;
            c = 0;
            split1 = i1;
            split2 = i2;
        }

        public Instruction(Instruction i)
        {
            OperationCode = Operation.Jmp;
            c = 0;
            split1 = null;
            split2 = null;
            next = i;
        }
    }
}
