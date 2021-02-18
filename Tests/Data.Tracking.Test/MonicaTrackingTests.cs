using Data.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Monica.Data.Test
{
    [TestClass]
    public class TrackingTests
    {
        private class TrackableTestObject : ITrackableObject
        {
            public Guid Id { get; set; }

            public int IntValue { get; set; }

            public string TextValue { get; set; }

            [NotMapped]
            public string NotMappedValue { get; set; }

            public TrackingState Tracking { get; set; }
        }

        [TestMethod]
        public void HasChangedTest()
        {
            TrackableTestObject context = new TrackableTestObject();

            context.StartTracking();
            Assert.IsFalse(context.HasChanges());

            context.IntValue = 2;
            Assert.IsTrue(context.HasChanges());


            context.StartTracking();
            Assert.IsFalse(context.HasChanges());

            context.NotMappedValue = "test";
            Assert.IsFalse(context.HasChanges());

            context.TextValue = "test";
            Assert.IsTrue(context.HasChanges());
        }

        [TestMethod]
        public void MergeTrackingEntityTest()
        {
            TrackableTestObject context1 = new TrackableTestObject();

            context1.StartTracking();
            Assert.IsFalse(context1.HasChanges());

            context1.IntValue = 2;
            Assert.IsTrue(context1.HasChanges());

            TrackableTestObject context2 = new TrackableTestObject();
            context2.MergeTracking(context1);

            //Have the changes from context 1 been merged onto context 2?
            //If the merge was successful we should see changes merged onto context 2
            Assert.AreEqual(context1.IntValue, context2.IntValue);
            Assert.IsTrue(context2.HasChanges());
        }

        [TestMethod]
        public void MergeTrackingEnumerableTest()
        {
            Guid[] ids = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            List<TrackableTestObject> contextList = new List<TrackableTestObject>()
            {
                new TrackableTestObject() { Id = ids[0], TextValue = "Modify 1" },
                new TrackableTestObject() { Id = ids[1], TextValue = "Modify 2" },
                new TrackableTestObject() { Id = ids[2], TextValue = "Delete 102" }
            };

            contextList.StartTracking();
            Assert.IsFalse(contextList.HasChanges());

            TrackableTestObject contextAdd = new TrackableTestObject() { Id = ids[3], TextValue = "Add 101" };
            contextList.Add(contextAdd);
            contextAdd.New();
            Assert.IsTrue(contextList.HasChanges());

            contextList.First(item => item.Id == ids[2]).Delete();


            List<TrackableTestObject> merge = new List<TrackableTestObject>()
            {
                new TrackableTestObject() { Id = ids[0], TextValue = "Modify 1" },
                new TrackableTestObject() { Id = ids[1], TextValue = "Modify 2" },
                new TrackableTestObject() { Id = ids[2], TextValue = "Delete 102" }
            };

            merge.MergeTracking(contextList);
            Assert.IsTrue(merge.HasChanges());

            Assert.AreEqual(4, merge.Count);

            Assert.IsTrue(merge.First(item => item.Id == ids[3]).IsNew());
            Assert.IsTrue(merge.First(item => item.Id == ids[2]).IsDeleted());

        }

        [TestMethod]
        public void PrintTrackingTest()
        {
            int changesDetected = 0;

            TrackableTestObject context = new TrackableTestObject();
            context.StartTracking();
            context.WhenChanged(() => changesDetected++);

            context.Print();

            Assert.IsFalse(context.HasChanges());           //We dont expect print to show up as having changes.
            Assert.IsTrue(context.IsPrintRequired());
            Assert.AreEqual(1, changesDetected);
        }
    }
}
