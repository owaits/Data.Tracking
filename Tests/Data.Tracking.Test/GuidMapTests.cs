using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oarw.Data.Tracking.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oarw.Data.Tracking.Test
{
    [TestClass]
    public class GuidMapTests
    {
        [TestMethod]
        public void EnsureIDRelationshipTest()
        {
            Guid testId = Guid.NewGuid();

            GuidMap map = new GuidMap();

            Assert.AreEqual(map.MapId(testId), map.MapId(testId));
            Assert.AreNotEqual(map.MapId(testId), map.MapId(Guid.NewGuid()));
        }

        [TestMethod]
        public void PreserveNullIDsTest()
        {
            Guid testId = Guid.NewGuid();

            GuidMap map = new GuidMap();
            Assert.AreEqual(null, map.MapId(null));
        }
    }
}
