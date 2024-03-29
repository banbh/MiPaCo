﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static MiPaCo.Combinators;
using static MiPaCoTests.Utilities;

namespace MiPaCo.Tests
{
    static class Extensions
    {
        public static Parser<T> LazyChainL1<T>(this Parser<T> p, Parser<Func<T, T, T>> op)
        {
            Parser<T> rest(T t) => grab(t).Or(Return(t)); // greedy
            Parser<T> grab(T t1) => from f in op
                                    from t2 in p
                                    from x in rest(f(t1, t2))
                                    select x;
            return p.Bind(rest);
        }
    }

    [TestClass()]
    public class CombinatorsTests
    {
        static IEnumerable<int> SeqFail(params int[] nn)
        {
            foreach (var n in nn)
            {
                yield return n;
            }
            Fail("Enumerable called too many times");
        }


        [TestMethod()]
        public void DefaultIfEmpty1Test()
        {
            List<int> one = new() { 1 };
            CollectionAssert.AreEqual(one, SeqFail(1).DefaultIfEmpty1(SeqFail()).ToList());
            CollectionAssert.AreEqual(one, Array.Empty<int>().DefaultIfEmpty1(SeqFail(1)).ToList());
        }

        static readonly Parser<char> AlphaNumOrDigit =
            (from _ in Char(char.IsLetterOrDigit) select 'L').Or
            (from _ in Char(char.IsDigit) select 'D');

        [TestMethod()]
        public void OrTest()
        {
            Parser<char> p = AlphaNumOrDigit.Or(Char('1'));
            That.AssertResults(p("1"), expectEmptyRest: true, 'L', 'D', '1');
        }

        [TestMethod()]
        public void DorTest()
        {
            var p = AlphaNumOrDigit.Dor(Char('1'));
            That.AssertResults(p("1"), expectEmptyRest: true, 'L'); // in x.Dor(y), if x returns multiple then it returns the first of them

            var q = Char('2').Dor(AlphaNumOrDigit);
            That.AssertResults(q("1"), expectEmptyRest: true, 'L'); // in x.Dor(y), if x returns nothing then it returns the first of y
        }

        [TestMethod()]
        public void ChainTest()
        {
            Parser<string> p = Char(char.IsLetter).Many1().Select(string.Concat).Token() // greedy
                .ChainL1(Return<Func<string, string, string>>((x, y) => $"({x}, {y})")); // greedy
            That.AssertResults(p("f  gg  hhh  iiii   "), expectEmptyRest: true, "(((f, gg), hhh), iiii)");

            Parser<string> q = AlphaNumOrDigit.Select(char.ToString).Token() // greedy
                .ChainL1(Return<Func<string, string, string>>((x, y) => $"({x}, {y})")); // greedy
            //q.ParseAndPrint("1   2      .");
            That.AssertResults(q("1  2 "), expectEmptyRest: true, "(L, L)", "(D, L)");
            // Why two? Because the final result comes from p.Bind(rest) so if p returns two parses we get both
            // but after that Dor() intervenes (I think) and filters it down to just one result.
        }

        [TestMethod()]
        public void LazyChainTest()
        {
            Parser<string> p = Char(char.IsLetter).Many1().Select(string.Concat).Token() // greedy
                .LazyChainL1(Return<Func<string, string, string>>((x, y) => $"({x}, {y})")); // lazy
            That.AssertResults(p("f  gg  hhh  iiii   "),
                expectEmptyRest: false,
                "(((f, gg), hhh), iiii)",
                "((f, gg), hhh)",
                "(f, gg)",
                "f");
        }

        [TestMethod()]
        public void ChainL1Test()
        {
            var op = Char('-').Select<char, Func<string, string, string>>(c => (x, y) => $"({x} {c} {y})");
            Parser<string> digit = Char(char.IsDigit).Select(char.ToString);
            Parser<string> p = digit.ChainL1(op);
            That.AssertResults(p("1"), true, "1");
            That.AssertResults(p("1-2-3-4"), true, "(((1 - 2) - 3) - 4)");
        }

        [TestMethod()]
        public void ChainR1Test()
        {
            var op = Char('^').Select<char, Func<string, string, string>>(c => (x, y) => $"({x} {c} {y})");
            Parser<string> digit = Char(char.IsDigit).Select(char.ToString);
            Parser<string> p = digit.ChainR1(op);
            That.AssertResults(p("1"), true, "1");
            That.AssertResults(p("1^2^3^4"), true, "(1 ^ (2 ^ (3 ^ 4)))");
        }
    }
}