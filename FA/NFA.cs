using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    class NFA
    {
        private NFAState start;
        private DFATable table = new DFATable();
        private int listid = 0;

        private NFA()
        {
            start = null;
        }
        private NFA(NFAState start_state)
        {
            start = start_state;
        }

        class ParenthesesGroup
        {
            public int natom, nalt;
        };

        /// <summary>
        /// Конструирует КА по регулярному выражению.
        /// </summary>
        /// <param name="re"></param>
        /// <returns></returns>
        public static NFA FromRe(string re)
        {
            return FromPostfixRe(InfixToPostfixRe(re));
        }

        /// <summary>
        /// Алгоритм сортировочной станции для РВ. Вставляет явный оператор конкатенации. 
        /// </summary>
        /// <param name="re">Регулярное выражение в инфиксной нотации</param>
        /// <returns></returns>
        private static string InfixToPostfixRe(string re)
        {
            const char CONCAT = '&';
            const char LEFTP = '(';
            const char RIGHTP = ')';
            const char ALT = '|';
            const char STAR = '*';
            const char PLUS = '+';
            const char QUEST = '?';

            LinkedList<Char> output = new LinkedList<Char>();
            Stack<Char> stack = new Stack<char>();

            /*
             * Две особые переменные: natom хранит сколько "атомарных" символов/операций 
             * (в данном случае это или символ или оп. конкатенации) подряд
             * встречалось во входном потоке непосредственно до текущего символа, для корректной вставки оп. конкатенации
             * nalt - хранит тоже самое для оп. ИЛИ
             */
            int natom = 0, nalt = 0;

            /* Как только парсер попадает в открытую скобку - natom и nalt нужно обнулить, так как
             * для конкатенации и для оп. ИЛИ выражение в скобаках является атомарным, 
             * а в нем в свою очередь могуть быть другие такие операции

             * Для этого испульзуется список: как только мы "заходим" в скобки - выделить новый элемент в списке
             * а текущие natom и nalts запомнить в списке.
             * Следовательно, как только "выходим" - 
             * вернутся на один элемент в списке назад и поставить nalts и natom в значения до "захода" в скобки.
             */
            LinkedList<ParenthesesGroup> groups = new LinkedList<ParenthesesGroup>();
            groups.AddFirst(new ParenthesesGroup());
            LinkedListNode<ParenthesesGroup> curGroup = groups.First;
            int charnum = 0;
            foreach (var c in re)
            {
                switch (c)
                {
                    case LEFTP:
                        //Если до "сейчас" было два атомарных выражения, самое время втсавить &
                        if (natom > 1)
                        {
                            output.AddLast(CONCAT);
                            natom--;
                        }
                        //...запомнить и сбросить значения natom и nalts
                        curGroup.Value.natom = natom;
                        curGroup.Value.nalt = nalt;
                        natom = 0;
                        nalt = 0;
                        if (curGroup.Next == null)
                            curGroup = groups.AddAfter(curGroup, new ParenthesesGroup());
                        else
                            curGroup = curGroup.Next;
                        break;
                    case RIGHTP:
                        if (natom == 0 || curGroup == groups.First)
                            throw new InvalidRegexException("Probably empty or invalid parentheses!", charnum);
                        //Если мы "выходим" из скобки - со стека нужно снять все операторы
                        while (--natom > 0)
                            output.AddLast(CONCAT);
                        for (;nalt > 0; nalt--)
                            output.AddLast(ALT);
                        //и вернуться "наружу", взяв записанные значения nalt и natom
                        curGroup = curGroup.Previous;
                        nalt = curGroup.Value.nalt;
                        natom = curGroup.Value.natom;
                        natom++;
                        break;
                    case ALT:
                        if (natom == 0)
                            throw new InvalidRegexException(ALT.ToString() + " operator has not enough operands", charnum);
                        while (--natom > 0)
                            output.AddLast(CONCAT);
                        nalt++;
                        break;
                    case STAR:
                    case PLUS:
                    case QUEST:
                        if (natom == 0)
                            throw new InvalidRegexException("Lonely " + c.ToString() + ": no valid expression before it.", charnum);
                        output.AddLast(c);
                        break;
                    default:
                        //Если встречается не мета-символ, вставить конкатенацию если набралось два атома
                        //И уменьшить кол-во атомов
                        if (natom > 1)
                        {
                            output.AddLast(CONCAT);
                            natom--;
                        }
                        //вытолкнуть его в выходной поток и увеличить кол-во атомов
                        output.AddLast(c);
                        natom++;
                        break;
                }
                charnum++;
            }
            if (curGroup != groups.First)
                throw new InvalidRegexException("Probably invalid parenthesis!", charnum);

            //По окончании символов входного потока в стеке могуть остаться операции - вытолкнуть их в выходной поток.
            while (--natom > 0)
                output.AddLast(CONCAT);
            for (;nalt > 0; nalt--)
                output.AddLast(ALT);
            
            StringBuilder res = new StringBuilder();
            foreach (var item in output)
            {
                res.Append(item);
            }
            return res.ToString();
        }

        /// <summary>
        /// Конструируем НКА по постфиксному выражению по алгоритму Томпсона
        /// </summary>
        /// <param name="re"></param>
        /// <returns></returns>
        private static NFA FromPostfixRe(string re)
        {
            /*
             * Так как выражение поступает в постфиксной нотации алгоритм работает на стеке.
             * Для построения НКА используются такие абстрации:
             * NFAState - состояние НКА с выходным(-и) узлом. Может быть четырех типов:
             *      если c < 256 - это состояние с одним выходом, по которому можно перейти если очередной символ равен (char)c
             *      если c = 256 - это состояние с двумя е-переходами (Split)
             *      и если c = 257 - то это финальное состояние (оно одно такое)
             * NFAFragment - кусочек строящегося НКА, со ссылкой на стартовое состояние и указателями (NFAStatePointer) на 
             * выходные узлы графа. 
             * Таким образом соединение двух фрагментов f1, f2 например для случая конкатенации (последовательно)
             * осуществляется путем создания нового фрагмента f3, установки f3.Start = f1.Start, f3.OutArrows = f2.OutArrows
             * и установки всех указателей из f1.OutArrows на f2.Start (NFAFragment.Patch())
             */
            Stack<NFAFragment> stack = new Stack<NFAFragment>();
            NFAFragment e, e1, e2;
            NFAState s;

            foreach (char ch in re)
            {
                switch (ch)
                {
                    default: //Literal characters
                        s = new NFAState(ch, null, null);

                        stack.Push(new NFAFragment(s, new List<NFAStatePointer>() { s.OutArrowPtr }));
                        break;
                    case '&': //Concatenation
                        e2 = stack.Pop();
                        e1 = stack.Pop();
                        NFAFragment.Patch(e1, e2.Start);

                        stack.Push(new NFAFragment(e1.Start, e2.OutArrows));
                        break;
                    case '|': //Alternation
                        e2 = stack.Pop();
                        e1 = stack.Pop();
                        s = new NFAState(NFAStateName.Split, e1.Start, e2.Start);

                        stack.Push(new NFAFragment(s, NFAFragment.Concat(e1, e2)));
                        break;
                    case '?': //Zero or one
                        e = stack.Pop();
                        s = new NFAState(NFAStateName.Split, e.Start, null);

                        stack.Push(new NFAFragment(s, NFAFragment.Concat(e.OutArrows, new List<NFAStatePointer>() { s.OutArrow1Ptr })));
                        break;

                    case '*': //Zero or more
                        e = stack.Pop();
                        s = new NFAState(NFAStateName.Split, e.Start, null);
                        NFAFragment.Patch(e, s);

                        stack.Push(new NFAFragment(s, new List<NFAStatePointer>() { s.OutArrow1Ptr }));
                        break;
                    case '+': //One or more
                        e = stack.Pop();
                        s = new NFAState(NFAStateName.Split, e.Start, null);
                        NFAFragment.Patch(e, s);

                        stack.Push(new NFAFragment(e.Start, new List<NFAStatePointer>() { s.OutArrow1Ptr }));
                        break;
                }
            }
            //Осталось только взять верхний элемент стека, он и есть построенный НКА, 
            //и все выходные связи установить в финальное состояние
            e = stack.Pop();
            NFAFragment.Patch(e, NFAState.MatchState);
            return new NFA(e.Start);
        }

        /// <summary>
        /// Проверка принадлежности строки s к языку НКА
        /// Моделирование работы НКА по алгоритму МакНотона-Ямады-Томпсона за O(rs)
        /// (r - длинна РВ, x - длинна входной строки)
        /// с кешированием состояний и переходом впоследствии к моделированию ДКА за O(s)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Match(string s)
        {
            /// Так как это НКА, то текущему символу входного потока может соответстсоввать несколько состояний КА.
            /// (так как существуют e-переходы, по которым НКА может двигаться независимо от входного символа)
            /// Алгоритм заключается в том, что для текущего входного символа и множества состояний
            /// мы вычисляем е-замыкание для каждого состояния и сдвигаемся в новое множество, т. е.
            /// мы двигаемся по всем допустимым из даного состояниям НКА одновременно
            /// и также проходим по всем e-переходам НКА, по которым из даного состояний возможно пройти.
            /// Для этого используется два списка clist - для отображения множества "текущих состояний" 
            /// и nlist - для "следующих состояний" при даном входном символе.
            /// clist инициализируется е-замыканием от стартового состояния при первом входном символе потока. 
            /// А дальше на каждом шаге берется следующий входной символ,
            /// и в список nlist вносится е-замыкание каждого состояния из clist при даном входном символе (если состония еще там нет).
            /// Дальше clist устанвавливается на nlist, nlist обнуляется и процесс повторяется, пока есть символы входного потока.
            /// Также, по мере вычисления множеств состояний, 
            /// они вносятся в кеш DFATable, и перед каждой попыткой приступить к следующему шагу
            /// вычисления новой пары множеств текущее-следующие, проверяется, нет ли такой в кеше.
            /// По сути, если алгоритм кешурет все множества НКА и двигается только по кешированым, он превращается в эквивалентный ДКА
            /// и моделирование становится пропорциональным только длинне строки. 
            /// Таким образом при небольшом замедлении моделирования (внесении в кеш состояний) вначале, когда еще мало состояний
            /// попало в кеш, впоследствии алгоритм переходит на наиболее эффектинове 
            /// время уже только пропорциональное длинне строки.
            List<NFAState> clist, nlist;
            clist = StartList(start);
            nlist = new List<NFAState>();
            foreach (var c in s)
            {
                if (!table.ContainsState(clist) || table.GetNextState(clist, c) == null)
                {
                    Step(clist, c, nlist);
                    if (table.ContainsState(clist))
                        table.UpdateState(clist, nlist, c);
                    else
                    {
                        table.AddNewState(clist, nlist, c);
                    }
                    clist = nlist;
                    nlist = new List<NFAState>();
                }
                else
                {
                    clist = table.GetNextState(clist, c);
                }
            }
            return isMatch(clist);
        }
        /// <summary>
        /// Конструирует начальное множество состояний, т. е. е-замыкание для стартового состояния s.
        /// Или берет его из кеша.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private List<NFAState> StartList(NFAState s)
        {
            listid++;
            if (table.StartState == null)
            {
                List<NFAState> l = new List<NFAState>();
                AddState(l, s);
                table.StartState = l;
                return l;
            }
            else
            {
                return table.StartState;
            }
        }

        /// <summary>
        /// Добавление е-замыкания состояния в "новый" список. 
        /// </summary>
        /// <param name="l"></param>
        /// <param name="s"></param>
        private void AddState(List<NFAState> l, NFAState s)
        {
            
            if (s == null || s.lastList == listid)
                return;
            s.lastList = listid;
            /*
             * Если NFAState находится в состоянии Split, то вносим состояния по выходным связям.
             * Т. е. двигаемся по е-переходам.
             */
            if (s.State == NFAStateName.Split)
            {
                AddState(l, s.OutArrowPtr.value);
                AddState(l, s.OutArrow1Ptr.value);
            }
            else
                l.Add(s);
        }

        /// <summary>
        /// Проверяет содержится ли в множестве финальное состояние
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        private bool isMatch(List<NFAState> stack)
        {
            return stack.Contains(NFAState.MatchState);
        }

        /// <summary>
        /// "Шаг" алгоритма Томпсона. Внесение в "новое" множество всех е-замыканий состояний из "старого".
        /// </summary>
        /// <param name="clist"></param>
        /// <param name="c"></param>
        /// <param name="nlist"></param>
        private void Step(List<NFAState> clist, char c, List<NFAState> nlist)
        {
            listid++;
            foreach (var state in clist)
            {
                if (state.c == c)
                {
                    AddState(nlist, state.OutArrowPtr.value);
                }
            }
        }
    }
}
