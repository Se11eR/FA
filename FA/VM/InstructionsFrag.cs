using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    class InstructionsFrag
    {
        public Instruction First;
        public Instruction Last;

        public InstructionsFrag(Instruction first, Instruction last)
        {
            First = first;
            Last = last;
        }
    }
}
