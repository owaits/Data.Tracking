using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oarw.Data.Tracking;
using Oarw.Data.Tracking.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Oarw.Data.Tracking.Test
{
    [TestClass]
    public class TrackingExtensionTests
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

        [TestMethod]
        public void TrackingChanges()
        {
            var testEntity = GenerateTestObjects(1).First();

            //Test changes to parent
            testEntity.StartTracking();
            testEntity.Name = "Probe 1";
            Assert.IsTrue(testEntity.HasChanges());

            //Test changes to single child
            testEntity.StartTracking();
            testEntity.Child.Name = "Probe 2";
            Assert.IsTrue(testEntity.HasChanges());

            //Test changes to multiple children.
            testEntity.StartTracking();
            testEntity.Children.First().Name = "Probe 3";
            Assert.IsTrue(testEntity.HasChanges());
        }

        [TestMethod]
        public void TrackingDelete()
        {
            var testEntity = GenerateTestObjects(1).First();

            //Parent Item Delete
            testEntity.StartTracking();
            testEntity.Delete();

            Assert.IsTrue(testEntity.IsModified(true));
            Assert.IsFalse(testEntity.IsModified(false));
            Assert.IsFalse(testEntity.Children.First().IsModified());

            Assert.IsTrue(testEntity.IsDeleted());
            Assert.IsFalse(testEntity.Children.First().IsDeleted());

            //Child Item Delete
            testEntity.StartTracking();
            testEntity.Children.First().Delete();

            Assert.IsTrue(testEntity.IsModified(true));
            //Assert.IsFalse(testEntity.IsModified(false));
            Assert.IsTrue(testEntity.Children.First().IsModified(true));

            Assert.IsTrue(testEntity.IsDeleted());
            Assert.IsTrue(testEntity.Children.First().IsDeleted());
        }
    }
}
