using System;
using static MiPaCo.Combinators;

namespace MiPaCo
{
    class Program
    {
        static void Main()
        {
            // Some basic examples
            var digit = Char(char.IsDigit);
            digit.ParseAndPrint("1a", $"{nameof(digit)}");
            digit.Many().Select(string.Concat).ParseAndPrint("123a", "digit.Many().Select(string.Concat)");
            Char(c => c != 'x').Many().Select(string.Concat).ParseAndPrint("abcxyz", "Char(c => c != 'x').Many()");

            Parser<string> digits(int n) => digit.N(n).Select(string.Concat);
            var separator = Char('-');
            (from yyyy in digits(4)
             from _ in separator
             from mm in digits(2)
             from __ in separator
             from dd in digits(2)
             select new DateTime(year: int.Parse(yyyy), month: int.Parse(mm), day: int.Parse(dd))).ParseAndPrint("2020-12-31.", "yyyymmdd");

            // Some more complex examples
            CalculatorExample.Main();
            SqlExample.Main();
        }
    }
}
