using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    class DFAState
    {
        private List<NFAState> nfa_states = null;
        private DFAState[] next = new DFAState[256];

        public DFAState(List<NFAState> ss)
        {
            nfa_states = ss;
        }

        public DFAState(List<NFAState> ss, char c, DFAState new_s)
            :this(ss)
        {
            DefineNewLink(c, new_s);
        }

        public List<NFAState> GetNextList(char c)
        {
            return next[c].nfa_states;
        }

        public void DefineNewLink(char c, DFAState new_s)
        {
            next[c] = new_s;
        }
    }
}
