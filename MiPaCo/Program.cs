using System;
using System.Collections.Immutable;
using System.Linq;
using static MiPaCo.Combinators;

namespace MiPaCo
{
    static class Program
    {
        /// <summary>Some basic examples</summary>
        static void BasicMain()
        {
            Console.WriteLine($"=== {nameof(BasicMain)} ===");

            var digit = Char(char.IsDigit);
            digit.ParseAndPrint("1a", $"{nameof(digit)}");
            digit.Many().Select(string.Concat).ParseAndPrint("123a", "digit.Many().Select(string.Concat)");
            Char(c => c != 'x').Many().Select(string.Concat).ParseAndPrint("abcxyz", "Char(c => c != 'x').Many()");

            (from c in AnyChar from _ in AnyChar from d in AnyChar select (c, d)).ParseAndPrint("abcd");

            Parser<int> digits(int n) => digit.N(n).Select(string.Concat).Select(int.Parse);
            var separator = Char('-');
            (from yyyy in digits(4)
             from _ in separator
             from mm in digits(2)
             from __ in separator
             from dd in digits(2)
             select new DateTime(year: yyyy, month: mm, day: dd)).ParseAndPrint("2020-12-31.", "yyyymmdd");
        }


        static Parser<ImmutableList<T>> LazyMany1<T>(this Parser<T> p) => from t in p from tt in p.LazyMany() select tt.Insert(0, t);
        static Parser<ImmutableList<T>> LazyMany<T>(this Parser<T> p) => p.LazyMany1().Or(Return(ImmutableList<T>.Empty));


        /// <summary>Some examples of lazy (as opposed to greedy) combinators.</summary>
        static void LazyMain()
        {
            Console.WriteLine($"=== {nameof(LazyMain)} ===");

            Parser<string> ident = Char(char.IsLetter).Many1().Select(string.Concat).Token(); // this is greedy
            Parser<string> idents = ident.LazyMany().Select(ss => string.Join("+", ss.Select(s => $"'{s}'")));
            idents.ParseAndPrint("a ab abc", nameof(idents)); // get 4 parses with varying amount left over
            var twoIdents = from x in idents from y in idents select (x, y);
            twoIdents.ParseAndPrint("a ab abc", nameof(twoIdents), "\n"); // 10 different parses with varying amounts left over
            twoIdents.ToEnd().ParseAndPrint(
                "a ab abc",
                $"{nameof(twoIdents)}.{nameof(Combinators.ToEnd)}",
                "\n"); // only 4 parses since it must, in effect, consume the whole input
        }

        static void Main()
        {
            BasicMain();
            LazyMain();

            // Some more complex examples
            CalculatorExample.Main();
            SqlExample.Main();
            LambdaCalculusExample.Main();
            LazyLambdaCalculusExample.Main();
        }
    }
}
