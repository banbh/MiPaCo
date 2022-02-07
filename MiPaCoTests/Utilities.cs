using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiPaCo;
using System.Collections.Generic;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace MiPaCoTests
{
    public class Utilities
    {
        public static void AssertResults<T>(IEnumerable<IResult<T>> results, params T[] expectedValues)
        {
            int i = 0;
            foreach (var result in results)
            {
                IsTrue(i < expectedValues.Length, "too many results returned {0} {1}", i, expectedValues.Length);
                AreEqual(expectedValues[i++], result.Value);
                AreEqual("", result.Rest);
            }
            AreEqual(expectedValues.Length, i, "Too few results were returned");
        }

        public static void AssertResultsIgnoreRest<T>(IEnumerable<IResult<T>> results, params T[] expectedValues)
        {
            int i = 0;
            foreach (var result in results)
            {
                IsTrue(i < expectedValues.Length, "too many results returned {0} {1}", i, expectedValues.Length);
                AreEqual(expectedValues[i++], result.Value);
                //AreEqual("", result.Rest);
            }
            AreEqual(expectedValues.Length, i, "Too few results were returned");
        }

    }
}
