using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    class Thread
    {
        Instruction pc;
        CharEnumerator sp;
        public Thread(Instruction _pc, CharEnumerator _sp)
        {
            pc = _pc;
            sp = _sp;
        }
    }
}
