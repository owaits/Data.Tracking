using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oarw.Data.Tracking.Extensions;
using Oarw.Data.Tracking.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Test
{
    [TestClass]
    public class FormatExtensionTests
    {
        /// <summary>
        /// Ensures that formatting time span with a requested interval works correctly.
        /// </summary>
        [TestMethod]
        public void FormatTimeSpanWithIntervalTest()
        {
            Assert.AreEqual("1 day", TimeSpan.FromDays(1).FormatTimeSpan(TimeSpan.FromDays(1)));
            Assert.AreEqual("2 days", TimeSpan.FromDays(2).FormatTimeSpan(TimeSpan.FromDays(1)));
            Assert.AreEqual("0.5 day", TimeSpan.FromDays(0.5).FormatTimeSpan(TimeSpan.FromDays(1)));

            Assert.AreEqual("1 hour", TimeSpan.FromHours(1).FormatTimeSpan(TimeSpan.FromHours(1)));
            Assert.AreEqual("2 hours", TimeSpan.FromHours(2).FormatTimeSpan(TimeSpan.FromHours(1)));
            Assert.AreEqual("24 hours", TimeSpan.FromHours(24).FormatTimeSpan(TimeSpan.FromHours(1)));
            Assert.AreEqual("0.5 hour", TimeSpan.FromHours(0.5).FormatTimeSpan(TimeSpan.FromHours(1)));
        }

        /// <summary>
        /// Ensures that formatting time span works correctly.
        /// </summary>
        [TestMethod]
        public void FormatTimeSpanTest()
        {
            Assert.AreEqual("1 day", TimeSpan.FromDays(1).FormatTimeSpan());
            Assert.AreEqual("2 days", TimeSpan.FromDays(2).FormatTimeSpan());
            Assert.AreEqual("12 hours", TimeSpan.FromDays(0.5).FormatTimeSpan());
            Assert.AreEqual("1 hour", TimeSpan.FromHours(1).FormatTimeSpan());
            Assert.AreEqual("23 hours", TimeSpan.FromHours(23).FormatTimeSpan());
            Assert.AreEqual("1 minute", TimeSpan.FromMinutes(1).FormatTimeSpan());
            Assert.AreEqual("10 minutes", TimeSpan.FromMinutes(10).FormatTimeSpan());
        }

    }
}
