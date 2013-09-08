using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FA
{
    public class VM
    {
        #region constants
        const char CONCAT = '&';
        const char LEFTP = '(';
        const char RIGHTP = ')';
        const char ALT = '|';
        const char STAR = '*';
        const char PLUS = '+';
        const char QUEST = '?';
        #endregion

        class ParenthesesGroup
        {
            public int natom, nalt;
        };

        /// <summary>
        /// Алгоритм сортировочной станции для РВ. Вставляет явный оператор конкатенации. 
        /// </summary>
        /// <param name="re">Регулярное выражение в инфиксной нотации</param>
        /// <returns></returns>
        private static string InfixToPostfixRe(string re)
        {

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
                        for (; nalt > 0; nalt--)
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
            for (; nalt > 0; nalt--)
                output.AddLast(ALT);

            StringBuilder res = new StringBuilder();
            foreach (var item in output)
            {
                res.Append(item);
            }
            return res.ToString();
        }

        public static Instruction GetInstStream(string re)
        {
            re = InfixToPostfixRe(re);
            Stack<InstructionsFrag> stack = new Stack<InstructionsFrag>();

            InstructionsFrag f, f1, f2;
            Instruction i, empty;

            foreach (char ch in re)
            {
                switch (ch)
                {
                    default: //Literal characters
                        i = new Instruction(ch);

                        stack.Push(new InstructionsFrag(i, i));
                        break;
                    case CONCAT: //Concatenation
                        f2 = stack.Pop();
                        f1 = stack.Pop();
                        f1.Last.next = f2.First;

                        stack.Push(new InstructionsFrag(f1.First, f2.Last));
                        break;
                    case ALT: //Alternation
                        f2 = stack.Pop();
                        f1 = stack.Pop();

                        i = new Instruction(f1.First, f2.First);
                        empty = new Instruction(null);
                        f1.Last.next = new Instruction(empty);
                        f2.Last.next = empty;
                        

                        stack.Push(new InstructionsFrag(i, empty));
                        break;
                    case QUEST: //Zero or one
                        f = stack.Pop();
                        empty = new Instruction(null);
                        i = new Instruction(f.First, empty);
                        f.Last.next = empty;

                        stack.Push(new InstructionsFrag(i, empty));
                        break;

                    case STAR: //Zero or more
                        f = stack.Pop();
                        empty = new Instruction(null);
                        i = new Instruction(f.First, empty);
                        f.Last.next = new Instruction(i);

                        stack.Push(new InstructionsFrag(i, empty));
                        break;
                    case PLUS: //One or more
                        f = stack.Pop();
                        empty = new Instruction(null);
                        i = new Instruction(f.First, empty);
                        f.Last.next = i;

                        stack.Push(new InstructionsFrag(f.First, empty));
                        break;
                }
            }
            f = stack.Pop();
            f.Last.next = Instruction.MatchInst;
            return f.First;
        }

        public static bool RecursiveLoop(Instruction start, CharEnumerator sp)
        {
            Instruction pc = start;
            while (true)
            {
                switch (pc.OperationCode)
                {
                    case Operation.Char:
                        if (pc.c != sp.Current)
                            return false;
                        pc = pc.next;
                        sp.MoveNext();
                        continue;
                    case Operation.Match:
                        return true;
                    case Operation.Jmp:
                        pc = pc.next;
                        continue;
                    case Operation.Split:
                        if (RecursiveLoop(pc.split1, sp))
                            return true;
                        pc = pc.split2;
                        continue;
                    default:
                        throw new ApplicationException("fuck");
                }
            }
        }

        public static bool ThompsonVM(Instruction start, string str)
        {

        }
    }
}
