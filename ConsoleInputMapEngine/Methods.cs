using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleInputMapEngine
{
    static class Methods
    {
        public static Random rand = new Random();

        public static Dictionary<string, Func<double, double, double>> PrimitiveDoubleFuncs { get; } = new Dictionary<string, Func<double, double, double>>
        {
            {"+", (a,b) => a+b },
            {"-", (a,b) => a-b },
            {"*", (a,b) => a*b },
            {"/", (a,b) => a/b },
            {"%", (a,b) => a%b },
            {"POW", Math.Pow },
            {"MAX", Math.Max },
            {"MIN", Math.Min },
        };

        public static string Bla(List<string> input)
            => "[" + string.Join(" ", input.ToArray()) + "]";

        public static string RandJNV(List<string> input)
        {
            string[] answers = new[] 
            {
                "Ja",
                "Nein",
                "Vielleicht",
                "Halt bloß die Schnauze",
                "Ja ne"
            };
            return answers[rand.Next(0, answers.Length)];
        }
    }
}
