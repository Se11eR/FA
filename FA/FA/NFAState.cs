using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    internal enum NFAStateName
    {
        Literal, Split, Match, Any
    }

    internal class NFAState
    {
        public int c;
        private NFAStatePointer _outArrowPtr;
        public NFAStatePointer OutArrowPtr { get { return _outArrowPtr; } }
        private NFAStatePointer _outArrow1Ptr;
        public NFAStatePointer OutArrow1Ptr { get { return _outArrow1Ptr; } }
        public int lastList;

        private static NFAState match_state = new NFAState(NFAStateName.Match, null, null);
        public static NFAState MatchState
        {
            get { return match_state; }
        }

        private NFAState(NFAState out1, NFAState out2)
        {
            _outArrowPtr = new NFAStatePointer(out1);
            _outArrow1Ptr = new NFAStatePointer(out2);
            lastList = 0;
            c = -1;
        }

        public NFAState() : this(null, null)
        {
            c = 1;
        }

        public NFAState(char ch, NFAState out1, NFAState out2)
            : this(out1, out2)
        {
            c = ch;
        }

        public NFAState(NFAStateName sname, NFAState out1, NFAState out2)
            : this(out1, out2)
        {
            switch (sname)
            {
                case NFAStateName.Literal:
                    throw new ArgumentException("sname cannot be NFAStateName.Literal");
                case NFAStateName.Split:
                    c = 256;
                    break;
                case NFAStateName.Match:
                    c = 257;
                    break;
                default:
                    break;
            }
        }

        public NFAStateName State
        {
            get
            {
                if (c == '.') return NFAStateName.Any;
                if (c < 256) return NFAStateName.Literal;
                if (c == 256) return NFAStateName.Split;
                if (c == 257) return NFAStateName.Match;
                else
                    throw new ApplicationException("Illegal parameter c");
            }
        }

        public override string ToString()
        {
            return State.ToString() + (State == NFAStateName.Literal ? ": " + ((char)c).ToString() : "");
        }
    }
}
