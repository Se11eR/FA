using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    internal class DFATable
    {
        Dictionary<List<NFAState>, List<NFAState>[]> hash =
            new Dictionary<List<NFAState>, List<NFAState>[]>();
        private List<NFAState> start;
        public List<NFAState> StartState 
        {
            get { return start; }
            set { start = value; if (value != null) AddNewState(value); }
        }
        
        public DFATable()
        {
            StartState = null;
        }

        public bool AddNewState(List<NFAState> state, List<NFAState> new_state, char c)
        {
            if (ContainsState(state))
            {
                return false;
            }
            var a = new List<NFAState>[256];
            a[c] = new_state;
            hash.Add(state, a);
            return true;
        }

        public bool AddNewState(List<NFAState> state)
        {
            if (ContainsState(state))
            {
                return false;
            }
            hash.Add(state, new List<NFAState>[256]);
            return true;
        }

        public bool ContainsState(List<NFAState> state)
        {
            return hash.ContainsKey(state);
        }

        public void UpdateState(List<NFAState> state, List<NFAState> new_state, char c)
        {
            var v = hash[state];
            v[c] = new_state;
        }

        public List<NFAState> GetNextState(List<NFAState> state, char c)
        {
            return hash[state][c];
        }
    }
}
