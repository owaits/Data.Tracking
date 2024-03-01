using Oarw.Data.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Oarw.Data.Tracking.TestData;

namespace Oarw.Data.Tracking.Test
{
    [TestClass]
    public class TrackingTests
    {
        [TestMethod]
        public void HasChangedTest()
        {
            TrackableObject context = new TrackableObject();

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
        
        /// <summary>
        /// Checks that a user can manually modify an entity.
        /// </summary>
        [TestMethod]
        public void ManuallyModifiedTest()
        {
            TrackableObject context = new TrackableObject();

            context.StartTracking();
            Assert.IsFalse(context.HasChanges());

            context.Modified();
            Assert.IsTrue(context.IsModified());
            Assert.IsTrue(context.HasChanges());
        }

        [TestMethod]
        public void MergeTrackingEntityTest()
        {
            TrackableObject context1 = new TrackableObject();

            context1.StartTracking();
            Assert.IsFalse(context1.HasChanges());

            context1.IntValue = 2;
            Assert.IsTrue(context1.HasChanges());

            var testSubItem = new TrackableSubObject() { Name = "Sub 1" };
            testSubItem.New();
            context1.Children.Add(testSubItem);
            Assert.IsTrue(context1.IsNew());

            Assert.IsTrue(context1.Children[0].HasChanges());
            Assert.IsTrue(context1.Children[0].IsNew());

            context1.Child = new TrackableSubObject() { Name = "Child 1" };
            context1.Child.New();

            TrackableObject context2 = new TrackableObject();
            context2.Children.Add(new TrackableSubObject() {  Name ="Sub 2"});
            context2.Children.Add(new TrackableSubObject() { Name = "Sub 3" });

            context2.MergeTracking(context1);

            //Have the changes from context 1 been merged onto context 2?
            //If the merge was successful we should see changes merged onto context 2
            Assert.AreEqual(context1.IntValue, context2.IntValue);
            Assert.IsTrue(context2.HasChanges());

            Assert.IsFalse(context2.Children[0].HasChanges());
            Assert.IsFalse(context2.Children[1].HasChanges());

            Assert.IsTrue(context2.Children[2].HasChanges());
            Assert.IsTrue(context2.Children[2].IsNew());

            Assert.IsNotNull(context2.Child);
            Assert.IsTrue(context2.Child.IsNew());

            var postMergeSubItem = new TrackableSubObject() { Name = "Sub 4" };
            postMergeSubItem.New();
            context2.Children.Add(postMergeSubItem);
            Assert.IsTrue(context2.IsNew());
        }

        [TestMethod]
        public void MergeTrackingEnumerableTest()
        {
            Guid[] ids = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            List<TrackableObject> contextList = new List<TrackableObject>()
            {
                new TrackableObject() { Id = ids[0], TextValue = "Modify 1" },
                new TrackableObject() { Id = ids[1], TextValue = "Modify 2" },
                new TrackableObject() { Id = ids[2], TextValue = "Delete 102" }
            };

            contextList.StartTracking();
            Assert.IsFalse(contextList.HasChanges());

            TrackableObject contextAdd = new TrackableObject() { Id = ids[3], TextValue = "Add 101" };
            contextList.Add(contextAdd);
            contextAdd.New();
            Assert.IsTrue(contextList.HasChanges());

            contextList.First(item => item.Id == ids[2]).Delete();


            List<TrackableObject> merge = new List<TrackableObject>()
            {
                new TrackableObject() { Id = ids[0], TextValue = "Modify 1" },
                new TrackableObject() { Id = ids[1], TextValue = "Modify 2" },
                new TrackableObject() { Id = ids[2], TextValue = "Delete 102" }
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

            TrackableObject context = new TrackableObject();
            context.StartTracking();
            context.WhenChanged(() => changesDetected++);

            context.Print();

            Assert.IsFalse(context.HasChanges());           //We dont expect print to show up as having changes.
            Assert.IsTrue(context.IsPrintRequired());
            Assert.AreEqual(1, changesDetected);
        }

        [TestMethod]
        public void WhenChangedTest()
        {
            bool hasChanged1 = false;
            bool hasChanged2 = false;
            bool hasChanged3 = false;


            TrackableObject context2 = new TrackableObject();
            context2.Children.Add(new TrackableSubObject() { Name = "Sub 2"});
            context2.Children.Add(new TrackableSubObject() { Name = "Sub 3" });

            context2.StartTracking();
            context2.WhenChanged(() => { hasChanged1 = true; });
            context2.WhenChanged(() => { hasChanged2 = true; });


            context2.Children.WhenChanged(() => { hasChanged3 = true; });

            var subItem3 = new TrackableSubObject() { Name = "Sub 3" };
            context2.Children.Add(subItem3);
            subItem3.New(context2);

            Assert.IsTrue(hasChanged1);
            Assert.IsTrue(hasChanged2);
            Assert.IsTrue(hasChanged3);
        }
    }
}
