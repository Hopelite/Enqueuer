using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enqueuer.Tests.Utilities.Wrappers
{
    /// <summary>
    /// Extends <see cref="Assert"/> class.
    /// </summary>
    public static class AssertWrapper
    {

        /// <summary>
        /// Asserts that all elements in <paramref name="expected"/> are contained in <paramref name="actual"/>.
        /// </summary>
        /// <typeparam name="T">Type of elements in <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="expected">Expected elements to be in the <paramref name="actual"/>.</param>
        /// <param name="actual">Actual elements.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> to compare the elements of <paramref name="expected"/> and <paramref name="actual"/>. If it's null, then the default comparer will be used.</param>
        public static void MultipleEquals<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer = null)
        {
            var actualComparer = comparer ?? EqualityComparer<T>.Default;
            foreach (var element in expected)
            {
                Assert.IsTrue(actual.Contains(element, actualComparer));
            }
        }

        /// <summary>
        /// Asserts that all elements in <paramref name="expected"/> are contained in <paramref name="actual"/> and no more others.
        /// </summary>
        /// <typeparam name="T">Type of elements in <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="expected">Expected elements to be in the <paramref name="actual"/>.</param>
        /// <param name="actual">Actual elements.</param>
        /// <param name="comparer"><see cref="IEqualityComparer{T}"/> to compare the elements of <paramref name="expected"/> and <paramref name="actual"/>. If it's null, then the default comparer will be used.</param>
        public static void StrictMultipleEquals<T>(IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T> comparer = null)
        {
            Assert.AreEqual(expected.Count(), actual.Count());
            MultipleEquals(expected, actual, comparer);
        }
    }
}
