using System;

namespace MiPaCo
{
    /// <summary>Extensions methods used only in the example code.</summary>
    public static class ExampleUtilities
    {
        public static void ParseAndPrint<T>(this Parser<T> p, 
            string s, 
            string msg = "",
            string sep = ";") => Console.WriteLine($"{msg}: \"{s}\" -> {string.Join(sep, p(s))}");
    }
}
