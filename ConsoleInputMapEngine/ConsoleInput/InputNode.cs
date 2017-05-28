using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleInputMapEngine.ConsoleInput
{
    class InputNode
    {
        private Func<List<string>, string> function;
        public Func<List<string>, string> Function {
            get
            {
                return function;
            }
            set
            {
                function = value;
                IsEnd = true;
            }
        }
        public bool IsEnd { get; private set; }

        public Dictionary<string, InputNode> Children { get; set; }

        public InputNode()
        {
            Function = a => $"[ERROR no {Function} assigned on {((a == null) ? "final argument" : "argument \"" + a[0] + "\"]")}";
            IsEnd = false;
        }

        public InputNode(Func<List<string>, string> function)
        {
            Function = function;
        }

        public string Eval(List<string> args)
        {
            if (Children == null)
                return Function(args);
            else if(args.Count == 0)
                return Function(null);
            else if (Children.Keys.Contains(args[0]))
                return Children[args[0]].Eval(args.Skip(1).ToList());
            else return Function(args);
        }

        public string RootEval(List<string> args, string validSeperator)
        {
            if (Children == null)
                return Function(args);
            else if (args.Count == 0)
                return Function(null);
            else if (Children.Keys.Contains(args[0]))
                return Children[args[0]].Eval(args.Skip(1).ToList());
            else return string.Join(validSeperator, args.ToArray());
        }

        public static void EvalArguments(List<string> args, out Dictionary<string, string> output, List<char> argumentIndicators)
        {
            Dictionary<string, string> argValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); 
            List<string> marker = new List<string>();
            args.ForEach(a =>
            {
                int index = a.IndexOfAny(argumentIndicators.ToArray());
                if (index != -1)
                {
                    argValuePairs.Add(a.Substring(0, index), a.Substring(index + 1, a.Length - index - 1));
                    marker.Add(a);
                }
            });
            marker.ForEach(m => args.Remove(m));
            output = argValuePairs;
        }

        public void Print(Action<string> printMethod, string indent, bool isLast, string key)
        {
            printMethod(indent);
            if (isLast)
            {
                printMethod("╚═>");
                indent += "   ";
            }
            else
            {
                printMethod("╠═>");
                indent += "║  ";
            }
            printMethod(key);

            int counter = 0;
            if (Children == null)
            {
                printMethod("[End]\n");
                return;
            }
            else if (IsEnd)
                printMethod("[SemiEnd]");
            printMethod("\n");
            Children.Values.ToList().ForEach(v => v.Print(printMethod, indent, counter == Children.Count -1 , Children.Keys.ToList()[counter++]));
        }
    }
}
