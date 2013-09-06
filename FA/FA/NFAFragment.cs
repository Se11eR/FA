using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    internal class NFAFragment
    {
        NFAState _start;
        public NFAState Start
        {
            get { return _start; }
        }

        private IList<NFAStatePointer> _outArrows;
        public IList<NFAStatePointer> OutArrows
        {
            get
            {
                return _outArrows;
            }
        }

        public NFAFragment()
        {
            _start = new NFAState();
        }
        public NFAFragment(NFAState start, IList<NFAStatePointer> outArrows)
        {
            _start = start;
            _outArrows = outArrows;
        }

        public override string ToString()
        {
            return "Frag=" + Start.ToString();
        }

        /// <summary>
        /// Установить выходные связи frag на state
        /// </summary>
        /// <param name="frag"></param>
        /// <param name="state"></param>
        public static void Patch(NFAFragment frag, NFAState state)
        {
            var e = frag._outArrows.GetEnumerator();
            for (int i = 0; i < frag._outArrows.Count; i++)
            {
                frag._outArrows[i].value = state;
            }
        }

        /// <summary>
        /// Склеить списки выходных связей двух фрагментов
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static IList<NFAStatePointer> Concat(NFAFragment f1, NFAFragment f2)
        {
            return Concat(f1.OutArrows, f2.OutArrows);
        }

        /// <summary>
        /// Склеить списки выходных связей
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static IList<NFAStatePointer> Concat(IList<NFAStatePointer> list1, IList<NFAStatePointer> list2)
        {
            IList<NFAStatePointer> new_list = new List<NFAStatePointer>();
            foreach (var item in list1)
            {
                new_list.Add(item);
            }
            foreach (var item in list2)
            {
                new_list.Add(item);
            }
            return new_list;
        }

    }
}
