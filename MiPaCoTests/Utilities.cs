using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiPaCo;
using System.Collections.Generic;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace MiPaCoTests
{
    public static class Utilities
    {
        public static void AssertResults<T>(this Assert _,
            IEnumerable<IResult<T>> results, bool expectEmptyRest = true, params T[] expectedValues)
        {
            int i = 0;
            foreach (var result in results)
            {
                IsTrue(i < expectedValues.Length, "too many results returned {0} {1}", i, expectedValues.Length);
                AreEqual(expectedValues[i++], result.Value);
                if (expectEmptyRest) AreEqual("", result.Rest);
            }
            AreEqual(expectedValues.Length, i, "Too few results were returned");
        }
    }
}
