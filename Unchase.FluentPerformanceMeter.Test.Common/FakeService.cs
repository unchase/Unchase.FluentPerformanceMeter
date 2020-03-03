﻿using System.Threading;

namespace Unchase.FluentPerformanceMeter.Test.Common
{
    /// <summary>
    /// Fake service class.
    /// </summary>
    public class FakeService
    {
        /// <summary>
        /// Fake method 1.
        /// </summary>
        public static void FakeMethod1()
        {
            for (int i = 0; i < 1000000; i++)
            {
                var t = i.ToString() + (i + 1).ToString();
            }
        }

        /// <summary>
        /// Fake method 2.
        /// </summary>
        /// <returns>
        /// Return string.
        /// </returns>
        public static string FakeMethod2(string parameter)
        {
            Thread.Sleep(1000);
            return $"Fake method result with parameter value = {parameter}";
        }
    }
}
