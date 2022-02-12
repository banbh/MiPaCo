using System;
using static MiPaCo.Combinators;

namespace MiPaCo
{
    public class CalculatorExample
    {
        static readonly Func<int, int, int> Plus = (x, y) => x + y, Minus = (x, y) => x - y, Mult = (x, y) => x * y, Div = (x, y) => x / y;

        static readonly Parser<Func<int, int, int>> MulOps = Symb("*").Select(_ => Mult).Or(Symb("/").Select(_ => Div));
        static readonly Parser<Func<int, int, int>> AddOps = Symb("+").Select(_ => Plus).Or(Symb("-").Select(_ => Minus));
        public static Parser<int> Expr => Term.ChainL1(AddOps); // make into property to deal with circular dependency
        static readonly Parser<int> Num = Char(char.IsDigit).Many1().Token().Select(string.Concat).Select(int.Parse);
        static readonly Parser<int> Factor = Num.Or(from b1 in Symb("(") from n in Expr from b2 in Symb(")") select n);
        static readonly Parser<int> Term = Factor.ChainL1(MulOps);

        public static void Main()
        {
            Console.WriteLine();
            Console.WriteLine($"=== {typeof(CalculatorExample)} ===");

            Expr.ParseAndPrint("10 - 2  *  ( 1 + 2 )  +  4 ", $"{nameof(Expr)}");
        }
    }
}
