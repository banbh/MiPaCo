using System;

namespace MiPaCo
{
    public static class ExampleUtilities
    {
        public static void ParseAndPrint<T>(this Parser<T> p, string s, string msg = "") => Console.WriteLine($"{msg}: \"{s}\" -> {string.Join(';', p(s))}");
    }
}
