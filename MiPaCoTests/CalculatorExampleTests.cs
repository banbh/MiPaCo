using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using static MiPaCoTests.Utilities;

namespace MiPaCo.Tests
{
    [TestClass()]
    public class CalculatorExampleTests
    {
        [TestMethod()]
        public void MainTest()
        {
            That.AssertResults(CalculatorExample.Expr("10 - 2  *  ( 1 + 2 )  +  4 "), expectEmptyRest: true, 10 - 2 * (1 + 2) + 4);
            That.AssertResults(CalculatorExample.Expr("10 - 2  *   1 + 2   +  4 "), expectEmptyRest: true, 10 - 2 * 1 + 2 + 4);
            That.AssertResults(CalculatorExample.Expr("(10 - 2)  *  ( 1 + 2 )  +  4 "), expectEmptyRest: true, (10 - 2) * (1 + 2) + 4);
            That.AssertResults(CalculatorExample.Expr("10-(2*1)+2+4"), expectEmptyRest: true, 10 - (2 * 1) + 2 + 4);
            IsFalse(CalculatorExample.Expr.ToEnd()("21-)34").Any());
        }
    }
}