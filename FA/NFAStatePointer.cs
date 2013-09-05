using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    class NFAStatePointer
    {
        public NFAState value;
        public NFAStatePointer()
        {
            value = null;
        }
        public NFAStatePointer(NFAState p)
        {
            value = p;
        }
    }
}
