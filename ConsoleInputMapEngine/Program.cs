using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ConsoleInputMapEngine.ConsoleInput;
using static ConsoleInputMapEngine.Methods;
using System.IO;

namespace ConsoleInputMapEngine
{
    class CS : Dictionary<string, InputNode>
    {
        public CS() : base(StringComparer.Ordinal) { }
    }

    class NCS : Dictionary<string, InputNode>
    {
        public NCS() : base(StringComparer.OrdinalIgnoreCase) { }
    }

    static class Extensions
    {
        public static T1 Reset<T1, T2> (this T1 t, ref T2 target, T2 value = default(T2))
        {
            target = value;
            return t;
        }
        public static T DAR<T>(this T t, Action<T> action)
        {
            action(t);
            return t;
        }
        public static string AddOrOverride<T1, T2>(this Dictionary<T1, T2> d, T1 key, T2 value)
        {
            if (d.ContainsKey(key))
            {
                d.Remove(key);
                d.Add(key, value);
                return "[Overridden]";
            }
            else
            {
                d.Add(key, value);
                return "[Succes]";
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            InputMap map = new InputMap(false, " ");

            //--------Temporäre Zählvaribale für Linq-Operationen--------
            int miscCounter = 0;
            //----------------------------------------------------------

            Dictionary<string, string> variables = new Dictionary<string, string>();

            map.NeutralFromatters.AddRange(new[] { ";" });
            map.Root = new InputNode()
            {
                Children = new Dictionary<string, InputNode>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Clear", new InputNode(a => { Console.Clear(); return "[Console Cleared]"; }) },
                    { "Freeze", new InputNode(a => "{" + string.Join(" ", a.ToArray()) + "}") },
                    { "Set", new InputNode(a => variables.AddOrOverride(a[0], a[1]) ) },
                    {
                        "Script",
                        new InputNode(a => 
                        {
                            string[] lines = null;
                            try
                            {
                                lines = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), a[0]));
                            }
                            catch (IOException)
                            {
                                return $"Path \"{a[0]}\" not found";
                            }
                            Array.ForEach(lines, l => map.Eval(l));
                            return "[Script iterated]";
                        })
                    },
                    {
                        "Get",
                        new InputNode(a => 
                        {
                            string result = "[Error]";
                            variables.TryGetValue(a[0], out result);
                            return result;
                        })
                    },
                    {
                        "Print",
                        new InputNode(a => 
                        {
                            Dictionary<string, string> additionalArgs;
                            InputNode.EvalArguments(a, out additionalArgs, map.ArgumentIndicators);
                            string temp;
                            string temp2;
                            bool newLine = true;
                            if (additionalArgs.TryGetValue("Color", out temp))
                                try
                                {
                                    temp = temp.ToLower();
                                    temp = new string(temp.ToCharArray().DAR(c => c[0] = c[0].ToString().ToUpper()[0]));
                                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), temp);
                                }
                                catch {Console.WriteLine($"Color {temp[0]} not found");}
                            if (additionalArgs.TryGetValue("newLine", out temp2))
                            {
                                if (StringComparer.OrdinalIgnoreCase.Equals("no", temp2))
                                    newLine = false;
                            }
                            string result = string.Join(" ", a.ToArray());
                            Console.Write(result + (newLine?"\n":" "));
                            Console.ForegroundColor = ConsoleColor.Gray;
                            return result;
                        })
                    },
                    {
                        "ForEach",
                        new InputNode(a => 
                        {
                            Dictionary<string, string> adds;
                            InputNode.EvalArguments(a, out adds, map.ArgumentIndicators);
                            string sep;
                            string func;
                            if (!adds.TryGetValue("seperator", out sep))
                                return "[ERROR]";
                            if (adds.TryGetValue("func", out func))
                            {
                                List<string> result = new List<string>();
                                a[0].ToList().ForEach(aa => result.Add(map.Eval(string.Join(" ", func.Split(new[]{sep}, StringSplitOptions.RemoveEmptyEntries)) + " " + aa)));

                                return string.Join(" ", result.ToArray());
                            }
                            else return "[ERROR]";
                        })
                    },
                    {
                        "For",
                        new InputNode(a => 
                        {
                            Dictionary<string, string> adds;
                            InputNode.EvalArguments(a, out adds, map.ArgumentIndicators);
                            string temp;
                            string func;
                            string iterator;
                            int l = 0;
                            int u = 10;
                            int step = 1;
                            if (adds.TryGetValue("l", out temp))
                                int.TryParse(temp, out l);
                            if (adds.TryGetValue("u", out temp))
                                int.TryParse(temp, out u);
                            if (adds.TryGetValue("step", out temp))
                                int.TryParse(temp, out step);
                            if (!adds.TryGetValue("Func", out func))
                                return "[ERROR]";
                            if (!adds.TryGetValue("iterator", out iterator))
                                return "[ERROR]";

                            for (; l < u; l+=step)
                            {
                                variables.AddOrOverride(iterator, l.ToString());
                                List<char> mutable = func.ToList();
                                if (mutable[0] == '[')
                                {
                                    mutable.RemoveAt(0);
                                    mutable.RemoveAt(mutable.LastIndexOf(']'));
                                }
                                map.Eval(new string(mutable.ToArray()));
			                }
                            return $"[Success, {u/step} iterations]";
                        })
                    },
                    {
                        "Hey",
                        new InputNode
                        {
                            Function = a => "Na?",
                            Children = new NCS
                            {
                                {"Hoe",new InputNode(a => "Ich glaub es hackt")},
                                {
                                    "Was",
                                    new InputNode
                                    {
                                        Children = new NCS
                                        {
                                            {
                                                "Geht",
                                                new InputNode
                                                {
                                                    Children = new NCS
                                                    {
                                                        { "Ab",new InputNode(a => "Wir feiern die ganze Nacht") },
                                                        { "?",new InputNode(a =>"Nix, un bei dir so?") }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Fuck",
                        new InputNode
                        {
                            Function = a => "Wie obszön",
                            Children = new NCS
                            {
                                { "Off",new InputNode(Methods.RandJNV) },
                                {
                                    "You",
                                    new InputNode
                                    {
                                        Function = Methods.RandJNV,
                                        Children = new NCS
                                        {
                                            { "Too", new InputNode(Methods.RandJNV) }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    {
                        "Array",
                        new InputNode
                        {
                            Children = new NCS
                            {
                                { "Reverse", new InputNode(a => { a.Reverse(); return string.Join(" ", a.ToArray()); }) },
                                { "Sort", new InputNode(a => string.Join(" ", a.DAR(b => b.Sort()).ToArray())) },
                                { "SortNumbers", new InputNode(a => string.Join(" ", a.OrderBy(b => { double result; double.TryParse(b, out result); return result; }).ToArray())) },
                                { "Serialize", new InputNode(a => JSON(a)) },
                                { "Deserialize", new InputNode(a => string.Join(" ", DeJSON<string[]>(a[0]))) },
                                { "Generate", new InputNode
                                    {
                                        Children = new NCS
                                        {
                                            { "Random", new InputNode(a =>
                                                {
                                                    int c = 10;
                                                    int l = 0;
                                                    int u = 10;
                                                    string temp;
                                                    Dictionary<string, string> additionalArgs;
                                                    InputNode.EvalArguments(a, out additionalArgs, map.ArgumentIndicators);
                                                    if (additionalArgs.TryGetValue("c", out temp))
                                                        int.TryParse(temp, out c);
                                                    if (additionalArgs.TryGetValue("l", out temp))
                                                        int.TryParse(temp, out l);
                                                    if (additionalArgs.TryGetValue("u", out temp))
                                                        int.TryParse(temp, out u);
                                                    if (c < 1 || (l > u)) return "0";
                                                    string[] result = new string[c];
                                                    for (int i = 0; i < c; i++)
                                                        result[i] = rand.Next(l,u+1).ToString();
                                                    return string.Join(" ", result);
                                                }
                                            )}
                                        }
                                    }
                                },
                                {
                                    "Operator",
                                    new InputNode
                                    {
                                        Children = PrimitiveDoubleFuncs.Values
                                        .Select<Func<double,double,double>, Func<List<string>, string>>(a => b =>
                                        {
                                            double temp;
                                            if (!double.TryParse(b[0], out temp))
                                                Console.WriteLine($"Value \"{b[0]}\" could not be parsed, using 0.0 instead");
                                            for (int i = 1; i < b.Count; i++)
                                            {
                                                double temp2;
                                                if (!double.TryParse(b[i], out temp2))
                                                    Console.WriteLine($"Value \"{b[i]}\" could not be parsed, using 0.0 instead");
                                                temp = a(temp, temp2);
                                            }
                                            return temp.ToString();
                                        })
                                        .Select(a => new InputNode(a))
                                        .Reset(ref miscCounter)
                                        .ToDictionary(a => PrimitiveDoubleFuncs.Keys.ToList()[miscCounter++], StringComparer.OrdinalIgnoreCase)
                                    }
                                },
                                {
                                    "ForEach",
                                    new InputNode(a =>
                                    {
                                        Dictionary<string, string> adds;
                                        InputNode.EvalArguments(a, out adds, map.ArgumentIndicators);
                                        string sep;
                                        string funcArgs;
                                        if (!adds.TryGetValue("seperator", out sep))
                                            return "[ERROR]";
                                        if (!adds.TryGetValue("func", out funcArgs))
                                            return "[Error: no func arguments]";
                                        else
                                        {
                                            List<string> result = new List<string>();
                                            a.ForEach(aa => result.Add(map.Eval(string.Join(" ", funcArgs.Split(new[]{ sep }, StringSplitOptions.RemoveEmptyEntries).Concat(new[]{ aa }).ToArray()))));
                                            return string.Join(" ", result.ToArray());
                                        }
                                    })
                                }
                            }
                        }
                    }
                }
            };

            map.Print(s => Console.Write(s));
            for (;;)
                try { map.Eval(Console.ReadLine()); }
                catch (Newtonsoft.Json.JsonException e) { Console.WriteLine(e); }
        }
    }
}
