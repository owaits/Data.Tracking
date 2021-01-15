using Microsoft.VisualStudio.TestTools.UnitTesting;
using Data.Tracking;
using Data.Tracking.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Data.Tracking.Test
{
    [TestClass]
    public class UnitTest1
    {
        public int TrackableObject { get; private set; }

        private List<TrackableObject> GenerateTestObjects(int objectCount)
        {
            List<TrackableObject> trackedItems = new List<TrackableObject>();
            for(int n=0;n< objectCount; n++)
            {
                var item = new TrackableObject()
                {
                    Id = Guid.NewGuid(),
                    Name= $"Test Name {n}"
                };

                item.Children.Add(new TrackableSubObject() { Id = Guid.NewGuid(), Name = "Child 1", Description = $"Child 1 of Parent {n}" });
                item.Children.Add(new TrackableSubObject() { Id = Guid.NewGuid(), Name = "Child 2", Description = $"Child 2 of Parent {n}" });

                trackedItems.Add(item);
            }

            return trackedItems;
        }

        [TestMethod]
        public void TrackingPerformanceTest()
        {
            List<TrackableObject> trackedItems = GenerateTestObjects(2000);

            var timer1 = Stopwatch.StartNew();
            trackedItems.StartTracking();
            timer1.Stop();
            Assert.IsTrue(timer1.ElapsedMilliseconds < 20, $"Start tracking took too long, it should have been under 20ms but actually took {timer1.Elapsed}");

            var timer2 = Stopwatch.StartNew();
            Assert.IsFalse(trackedItems.Any(item => item.IsModified()));
            timer2.Stop();
            Assert.IsTrue(timer2.ElapsedMilliseconds < 6, $"IsModified tracking took too long, it should have been under 6ms but actually took {timer2.Elapsed}");

            var timer3 = Stopwatch.StartNew();
            Assert.IsFalse(trackedItems.Any(item => item.IsNew()));
            timer3.Stop();
            Assert.IsTrue(timer3.ElapsedMilliseconds < 5, $"IsNew tracking took too long, it should have been under 5ms but actually took {timer3.Elapsed}");

            var timer4 = Stopwatch.StartNew();
            Assert.IsFalse(trackedItems.Any(item => item.IsDeleted()));
            timer4.Stop();
            Assert.IsTrue(timer4.ElapsedMilliseconds < 5, $"IsDeleted tracking took too long, it should have been under 5ms but actually took {timer4.Elapsed}");

            var timer5 = Stopwatch.StartNew();
            trackedItems.StopTracking();
            timer5.Stop();
            Assert.IsTrue(timer5.ElapsedMilliseconds < 5, $"Stop tracking took too long, it should have been under 5ms but actually took {timer5.Elapsed}");

        }
    }
}
