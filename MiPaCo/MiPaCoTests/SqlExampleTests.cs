using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using static MiPaCo.SqlExample;

namespace MiPaCo.Tests
{
    [TestClass()]
    public class SqlExampleTests
    {
        [TestMethod()]
        public void ResultEqualsTest()
        {
            Assert.AreEqual("abc".Result("xyz"), "abc".Result("xyz"));
            Assert.AreNotEqual("abc".Result("xyz"), "abc".Result("xy"));
            Assert.AreNotEqual("abc".Result("xyz"), "ab".Result("xyz"));
        }

        static void ResultsAreEqual<T>(T expected, string rest, IEnumerable<IResult<T>> actual)
        {
            CollectionAssert.AreEqual(new[] { expected.Result(rest) }, actual.ToArray());
        }

        static readonly ComparisonPred
            a = new ComparisonPred(new IdentifierExpr("a"), Operator.Eq, new NumericExpr(1)),
            b = new ComparisonPred(new IdentifierExpr("b"), Operator.Eq, new NumericExpr(2)),
            c = new ComparisonPred(new IdentifierExpr("c"), Operator.Eq, new NumericExpr(3));

        [TestMethod()]
        public void DisjunctionsPAndAssociativityTest()
        {
            ResultsAreEqual(new AndPred(new AndPred(a, b), c), ";", // (a=1 and b=2) and c=3
                DisjunctionsP()("a=1 and b=2 and c=3 ;"));
        }

        [TestMethod()]
        public void SymbiTest()
        {
            ResultsAreEqual("and", ";", Symbi("and")("and;"));
            ResultsAreEqual("AND", ";", Symbi("and")("AND;"));
        }

        [TestMethod()]
        public void DisjunctionsPOrAssociativityTest()
        {
            ResultsAreEqual(new OrPred(new OrPred(a, b), c), ";", // (a=1 or b=2) or c=3
                DisjunctionsP()("a=1 or b=2 OR c=3 ;"));
        }

        [TestMethod()]
        public void DisjunctionsPPrecedenceTest()
        {
            ResultsAreEqual(new OrPred(a, new AndPred(b, c)), ";",  // a=1 or (b=2 and c=3)
                DisjunctionsP()("a=1 or b=2 and c=3 ;"));
        }

        [TestMethod()]
        public void DisjunctionsPParensTest()
        {
            ResultsAreEqual(new AndPred(new OrPred(a, b), c), ";",
                DisjunctionsP()("(a=1 or b=2) and c=3 ;"));
        }
    }
}