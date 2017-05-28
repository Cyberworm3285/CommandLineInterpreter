﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ConsoleInputMapEngine.ConsoleInput
{
    class InputMap
    {
        public InputNode Root { get; set; } 
        public List<string> Seperators { get; set; }
        public List<char> ForwarderOpenings { get; set; }
        public List<char> ForwarderFinishers { get; set; }
        public List<char> IgnoreOpenings { get; set; }
        public List<char> IgnoreFinishers { get; set; }
        public List<char> ArgumentIndicators { get; set; }
        public List<string> NeutralFromatters { get; set; }
        public StringSplitOptions SplitOptions { get; set; }
        protected string Cache { get; set; }

        public InputMap(bool caseSensitve, params string[] seperators)
        {
            Seperators = seperators.ToList();
            ForwarderOpenings = new List<char> { '{' };
            ForwarderFinishers = new List<char> { '}' };
            NeutralFromatters = new List<string> { "_", "~" };
            ArgumentIndicators = new List<char>() { ':', '=' };
            IgnoreOpenings = new List<char> { '[' };
            IgnoreFinishers = new List<char> { ']' };
            SplitOptions = StringSplitOptions.RemoveEmptyEntries;

        }

        public string Eval(string input)
        {
            if (input == null || input.Equals(string.Empty)) return "[ERROR]";
            NeutralFromatters.ForEach(f => input = input.Replace(f, ""));

            if (!Validate(input))
            {
                Console.WriteLine("Invalid Formatting");
                return "[ERROR]";
            }
            string old;
            Censor(input, out input, out old);
            int index = input.IndexOfAny(ForwarderFinishers.ToArray());
            while(index != -1)
            {
                int i;
                for (i = index; i >= 0; i--) //vom index zurück um den passenden anfang zu finden
                    if (ForwarderOpenings.Contains(input[i]))
                        break;
                string temp = Root.RootEval(old.Substring(i + 1, index - i - 1).Split(Seperators.ToArray(), SplitOptions).ToList(), Seperators[0]); // der alte beinhaltet die unzensierten argument-funtkionen

                NeutralFromatters.ForEach(n => temp = temp.Replace(n, ""));

                input = input.Remove(i, index - i + 1);
                old = old.Remove(i, index - i + 1);

                if (i != 0)
                {
                    old = old.Insert(i, Seperators[0] + temp + Seperators[0]);
                    input = input.Insert(i, Seperators[0] + temp + Seperators[0]);
                }
                else
                {
                    old = old.Insert(i, temp + Seperators[0]); // damit am anfang kein seperator is
                    input = input.Insert(i, temp + Seperators[0]);
                }
                Censor(old, out input, out old); // mögliche neue statische blöcke aufgetaucht
                index = input.IndexOfAny(ForwarderFinishers.ToArray());
            }
            //zB:
            //for iterator:i func:[for iterator:j func:[print{Get i}{Get j}]]
            return Root.RootEval(SplitArgs(old, input), Seperators[0]);
        }

        private void Censor(string input, out string censored, out string modified)
        {
            censored = input;
            modified = input;
            Stack<int> b = new Stack<int>();
            for (int i = 0; i < input.Length; i++) // Zensieren
            {
                if (IgnoreOpenings.Contains(input[i]))
                    b.Push(i);
                else if (IgnoreFinishers.Contains(input[i]))
                {
                    if (b.Count == 1)
                    {
                        modified = modified.Remove(b.Peek(), 1);
                        modified = modified.Insert(b.Peek(), NeutralFromatters[0]);
                        modified = modified.Remove(i, 1);
                        modified = modified.Insert(i, NeutralFromatters[0]);
                    }
                    censored = censored.Remove(b.Peek(), i - b.Peek() + 1);
                    censored = censored.Insert(b.Peek(), new string('X', i - b.Pop() + 1));
                }
            }
        }

        private bool Validate(string input)
        {
            Stack<char> stack = new Stack<char>();
            for (int i = 0; i < input.Length; i++)
            {
                if (ForwarderOpenings.Contains(input[i]))
                    stack.Push(input[i]);
                else if (ForwarderFinishers.Contains(input[i]))
                    if (stack.Count == 0)
                        return false;
                    else
                        stack.Pop();
            }
            return (stack.Count == 0);
        }

        private List<string> SplitArgs(string input, string censored)
        {
            input += Seperators[0];
            censored += Seperators[0]; // damit der weiß wann schluss is
            bool hasStart = true;
            int start = 0;
            List<string> result = new List<string>();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == censored[i] && Seperators.Contains(input[i].ToString()) && hasStart)
                {
                    result.Add(input.Substring(start, i - start));
                    while (i < input.Length && Seperators.Contains(input[i].ToString())) i++;
                    start = i;
                }
            }

            return result;
        }

        public void Print(Action<string> printMethod)
        {
            Root.Print(printMethod, "", true, "[START]");
        }
    }
}