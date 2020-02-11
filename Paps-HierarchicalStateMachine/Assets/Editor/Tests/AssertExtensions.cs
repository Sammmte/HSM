using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public static class AssertExtensions
    {
        public static void DoesNotContains(object expected, ICollection collection)
        {
            try
            {
                Assert.Contains(expected, collection);
            }
            catch(AssertionException)
            {
                return;
            }

            throw new AssertionException("Expected collection not to has " + expected);
        }
        
        public static void DoesNotContains<T>(T expected, IEnumerable<T> enumerable)
        {
            if(enumerable.Contains(expected))
                throw new AssertionException("Expected collection not to has " + expected);
        }

        public static void Contains<T>(T expected, IEnumerable<T> enumerable)
        {
            if (enumerable.Contains(expected) == false)
                throw new AssertionException("Enumerable does not contains expected value " + expected);
        }
    }
}