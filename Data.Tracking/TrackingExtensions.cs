using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Linq;

namespace Oarw.Data.Tracking
{
    public static class TrackingExtensions
    {
        private static Dictionary<ITrackableObject, TrackingState> trackedObjects = new Dictionary<ITrackableObject, TrackingState>();

        public static void StartTracking(this IEnumerable<ITrackableObject> source)
        {
            TrackingCache propertyCache = new TrackingCache();
            foreach (ITrackableObject item in source)
                StartTracking(item, propertyCache);
        }

        public static TrackingState StartTracking(this ITrackableObject source)
        {
            TrackingCache cache = new TrackingCache();
            return StartTracking(source, cache);
        }

        private static TrackingState StartTracking(ITrackableObject source, TrackingCache cache)
        {
            TrackingState tracker = new TrackingState(source);

            PropertyInfo[] properties;
            if (!cache.Properties.TryGetValue(source.GetType(), out properties))
            {
                properties = source.GetType().GetProperties();
                cache.Properties[source.GetType()] = properties;
            }

            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(TrackingState) && ShouldAllowProperty(property, cache))
                {
                    var value = property.GetValue(source);

                    if (property.PropertyType.IsGenericType && property.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                    {
                        System.Collections.IEnumerable list = (System.Collections.IEnumerable)value;
                        

                        List<ITrackableObject> deletedItems = new List<ITrackableObject>();
                        foreach (object item in list)
                        {
                            ITrackableObject trackedItem = item as ITrackableObject;
                            if (trackedItem != null)
                            {
                                if(trackedItem.IsTracking() && trackedItem.IsDeleted())
                                {
                                    deletedItems.Add(trackedItem);
                                    StopTracking(trackedItem);
                                }
                                else
                                {
                                    StartTracking(trackedItem, cache);
                                }                                
                            }
                        }

                        //Remove any items we detected as having been deleted.
                        IList editableList = list as IList;
                        if(editableList != null)
                        {
                            foreach (var removeItem in deletedItems)
                                editableList.Remove(removeItem);
                        }
                    }
                    else if (property.PropertyType.IsAssignableTo(typeof(ITrackableObject)))
                    {
                        ITrackableObject trackedItem = value as ITrackableObject;
                        if (trackedItem != null)
                        {
                            StartTracking(trackedItem, cache);
                        }
                    }

                    tracker.UnmodifiedState.Add(property, value);
                }
            }

            trackedObjects[source] = tracker;
            return tracker;
        }

        private static bool ShouldAllowProperty(PropertyInfo property)
        {
            var trackingAttribute = property.GetCustomAttribute<TrackingAttribute>();
            if (trackingAttribute != null)
            {
                return !trackingAttribute.Ignore;
            }

            return property.GetCustomAttribute<NotMappedAttribute>() == null;
        }

        private static bool ShouldAllowProperty(PropertyInfo property, TrackingCache cache)
        {
            bool result;
            if (cache.AllowedProperties.TryGetValue(property, out result))
                return result;

            result = ShouldAllowProperty(property);
            cache.AllowedProperties[property] = result;
            return result;
        }

        public static void StopTracking(this IEnumerable<ITrackableObject> source)
        {
            foreach (ITrackableObject item in source)
                item.StopTracking();
        }

        public static void StopTracking(this ITrackableObject source)
        {
            if (trackedObjects != null)
            {
                trackedObjects.Remove(source);
            }
        }

        public static IList<TEntity> MergeTracking<TEntity>(this IList<TEntity> target, IList<TEntity> source) where TEntity : ITrackableObject
        {
            //Go through the new data and apply any modifications to the target.
            foreach (ITrackableObject targetItem in target)
            {
                ITrackableObject sourceItem = null;

                if (source != null)
                    sourceItem = source.FirstOrDefault(item => item.Id == targetItem.Id);

                if (sourceItem != null)
                {
                    targetItem.MergeTracking(sourceItem);
                }
                else
                {
                    targetItem.StartTracking();
                }
            }

            //Go through the deleted items and apply them to the new data.
            if (source != null)
            {
                foreach (ITrackableObject sourceItem in source.Where(item => item.IsDeleted()))
                {
                    ITrackableObject targetItem = target.FirstOrDefault(item => item.Id == sourceItem.Id);
                    if (targetItem != null)
                    {
                        targetItem.Delete();
                    }
                }
            }

            //Go through the added items and apply them to the new data.
            if (source != null)
            {
                foreach (ITrackableObject sourceItem in source.Where(item => item.IsNew()))
                {
                    target.Add((TEntity)sourceItem);
                }
            }

            return target;
        }

        //public static void MergeTracking<TEntity>(this System.Collections.IEnumerable target, System.Collections.IEnumerable source)
        //{
        //    //Go through the new data and apply any modifications to the target.
        //    foreach (ITrackableObject targetItem in target)
        //    {
        //        ITrackableObject sourceItem = null;

        //        if (source != null)
        //            sourceItem = source.FirstOrDefault(item => item.Id == targetItem.Id);

        //        if (sourceItem != null)
        //        {
        //            targetItem.MergeTracking(sourceItem);
        //        }
        //        else
        //        {
        //            targetItem.StartTracking();
        //        }
        //    }

        //    //Go through the deleted items and apply them to the new data.
        //    if (source != null)
        //    {
        //        foreach (ITrackableObject sourceItem in source.Where(item => item.IsDeleted()))
        //        {
        //            ITrackableObject targetItem = target.FirstOrDefault(item => item.Id == sourceItem.Id);
        //            if (targetItem != null)
        //            {
        //                targetItem.Delete();
        //            }
        //        }
        //    }

        //    ////Go through the added items and apply them to the new data.
        //    //if (source != null)
        //    //{
        //    //    foreach (ITrackableObject sourceItem in source.Where(item => item.IsNew()))
        //    //    {
        //    //        target.Add((TEntity)sourceItem);
        //    //    }
        //    //}

        //    return target;
        //}

        public static TrackingState MergeTracking(this ITrackableObject target, ITrackableObject source)
        {
            TrackingState targetTracker = new TrackingState(target);
            TrackingState sourceTracker = GetTracker(source);

            foreach (var property in target.GetType().GetProperties())
            {
                
                if (property.PropertyType != typeof(TrackingState) && ShouldAllowProperty(property))
                {
                    targetTracker.UnmodifiedState.Add(property, property.GetValue(target));

                    if (property.PropertyType.IsGenericType && property.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                    {
                        dynamic targetList = property.GetValue(target);
                        if (targetList != null)
                        {
                            dynamic sourceList = property.GetValue(source);
                            MergeTracking(targetList, sourceList);
                        }
                    }
                    else if (property.PropertyType.IsAssignableTo(typeof(ITrackableObject)))
                    {
                        ITrackableObject sourceItem = property.GetValue(source) as ITrackableObject;
                        if (sourceItem != null)
                        {
                            if (sourceItem.IsNew() || sourceItem.IsDeleted())
                            {
                                property.SetValue(target, sourceItem);
                            }
                            else
                            {
                                ITrackableObject trackedItem = property.GetValue(target) as ITrackableObject;
                                if (trackedItem != null)
                                {
                                    trackedItem.MergeTracking(sourceItem);
                                }
                            }
                        }
                    }
                    else if (sourceTracker.UnmodifiedState.TryGetValue(property, out object sourceUnmodifiedValue))
                    {
                        if (IsPropertyModified(source, property, sourceUnmodifiedValue))
                            property.SetValue(target, property.GetValue(source));
                    }
                }
            }

            trackedObjects[target] = targetTracker;
            return targetTracker;
        }

        /// <summary>
        /// Updates a tracked entity with new values from the source without considering any differences as changes.
        /// </summary>
        /// <remarks>
        /// Use when you want to update the values on an existing tracked entity with new values that are not changes to b tracked.
        /// </remarks>
        /// <param name="target">The target enitity that is being tracked and that should be updated.</param>
        /// <param name="source">An enttity that is not being tracked but contains newer values than the target..</param>
        /// <returns>The updated tracking state.</returns>
        public static IList<TEntity> UpdateTracking<TEntity>(this IList<TEntity> target, IList<TEntity> source) where TEntity : ITrackableObject
        {
            //Go through the new data and apply any modifications to the target.
            foreach (ITrackableObject targetItem in target)
            {
                ITrackableObject sourceItem = null;

                if (source != null)
                    sourceItem = source.FirstOrDefault(item => item.Id == targetItem.Id);

                if (sourceItem != null)
                {
                    targetItem.UpdateTracking(sourceItem);
                }
            }

            return target;
        }

        /// <summary>
        /// Updates a tracked entity with new values from the source without considering any differences as changes.
        /// </summary>
        /// <remarks>
        /// Use when you want to update the values on an existing tracked entity with new values that are not changes to b tracked.
        /// </remarks>
        /// <param name="target">The target enitity that is being tracked and that should be updated.</param>
        /// <param name="source">An enttity that is not being tracked but contains newer values than the target..</param>
        /// <returns>The updated tracking state.</returns>
        public static TrackingState UpdateTracking(this ITrackableObject target, ITrackableObject source)
        {
            TrackingState targetTracker = GetTracker(target);

            foreach (var property in target.GetType().GetProperties())
            {

                if (property.PropertyType != typeof(TrackingState) && ShouldAllowProperty(property))
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                    {
                        dynamic targetList = property.GetValue(target);
                        if (targetList != null)
                        {
                            dynamic sourceList = property.GetValue(source);
                            UpdateTracking(targetList, sourceList);
                        }
                    }
                    else if (property.PropertyType.IsAssignableTo(typeof(ITrackableObject)))
                    {
                        ITrackableObject sourceItem = property.GetValue(source) as ITrackableObject;
                        if (sourceItem != null)
                        {
                            ITrackableObject trackedItem = property.GetValue(target) as ITrackableObject;
                            if (trackedItem != null)
                            {
                                trackedItem.UpdateTracking(sourceItem);
                            }
                        }
                    }
                    else if (targetTracker.UnmodifiedState.TryGetValue(property, out object targetUnmodifiedValue) && !IsPropertyModified(target, property, targetUnmodifiedValue))
                    {
                        //Only update properties that have no changes so the update does not clear out unsaved modifications.
                        //If there are no modifications then copy across the source value to the target without effecting the modified state.

                        var sourceValue = property.GetValue(source);
                        property.SetValue(target, sourceValue);                     //Copy across the source value to the target.
                        targetTracker.UnmodifiedState[property] = sourceValue;      //Update the unmodified tracked state so that the new value is not seen as a change.
                    }
                }
            }

            trackedObjects[target] = targetTracker;
            return targetTracker;
        }

        public static bool IsTracking(this IEnumerable<ITrackableObject> source)
        {
            return source.All(item => item.IsTracking());
        }

        public static bool IsTracking(this ITrackableObject source)
        {
            if (source == null)
                return false;

            return trackedObjects?.ContainsKey(source) ?? false;
        }

        private static TrackingState GetTracker(ITrackableObject source)
        {
            TrackingState tracker;
            if (!trackedObjects.TryGetValue(source, out tracker))
                throw new InvalidOperationException("Unable to detect changes because object is not being tracked. Please call StartTracking() first.");
            return tracker;
        }

        public static void Delete(this ITrackableObject source)
        {
            TrackingState tracker = GetTracker(source);
            tracker.Deleted = true;
        }

        /// <summary>
        /// Flags this entity as having been modified even when no changes have been detected.
        /// </summary>
        /// <param name="source">The trackable object to mark as modified.</param>
        public static void Modified(this ITrackableObject source)
        {
            TrackingState tracker = GetTracker(source);
            tracker.Modified = true;
        }

        public static void New(this ITrackableObject source, ITrackableObject parent = null)
        {
            var tracker = source.StartTracking();           
            if(parent != null)
            {
                var parentTracker = GetTracker(parent);
                foreach(var subscriber in parentTracker.OnChanged)
                    tracker.OnChanged.Add(subscriber);
            }
            tracker.Added = true;
        }

        public static bool HasChanges(this IEnumerable<ITrackableObject> source)
        {
            return source.Any(item => item.HasChanges());
        }

        public static void Undo(this IEnumerable<ITrackableObject> source)
        {
            foreach (var item in source)
                item.Undo();
        }

        public static void Undo(this ITrackableObject source)
        {
            TrackingState tracker = GetTracker(source);
            tracker.Deleted = false;
            tracker.Added = false;
            tracker.Modified = false;
            tracker.Print = false;
            foreach (var property in tracker.UnmodifiedState)
            {
                if (property.Value == null)
                {
                    if (property.Key.GetValue(source) != null)
                        property.Key.SetValue(source, null);
                }
                else
                {
                    if (!property.Value.Equals(property.Key.GetValue(source)))
                        property.Key.SetValue(source, property.Value);
                }
            }
        }


        /// <summary>
        /// Allows you to register a delegate that is called when the tracked objects change.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="onChangedDelegate">The on changed delegate.</param>
        public static void WhenChanged(this IEnumerable<ITrackableObject> source, Action onChangedDelegate)
        {
            if (source == null)
                return;

            TrackingCache cache = new TrackingCache();
            foreach (ITrackableObject item in source)
            {
                WhenChanged(item, onChangedDelegate, cache);
            }
        }

        public static void WhenChanged(this ITrackableObject item, Action onChangedDelegate)
        {
            if (item == null)
                return;

            TrackingCache cache = new TrackingCache();
            WhenChanged(item, onChangedDelegate, cache);
        }

        internal static void WhenChanged(ITrackableObject item, Action onChangedDelegate, TrackingCache cache)
        {
            if (onChangedDelegate == null)
                throw new ArgumentNullException(nameof(onChangedDelegate));

            TrackingState tracker = GetTracker(item);
            tracker.OnChanged.Add(onChangedDelegate);

            PropertyInfo[] properties;
            if (!cache.Properties.TryGetValue(item.GetType(), out properties))
            {
                properties = item.GetType().GetProperties();
                cache.Properties[item.GetType()] = properties;
            }

            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(TrackingState) && ShouldAllowProperty(property, cache))
                {
                    var value = property.GetValue(item);
                    if (property.PropertyType.IsGenericType && property.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                    {
                        System.Collections.IEnumerable list = (System.Collections.IEnumerable)value;
                        foreach (object subItem in list)
                        {
                            ITrackableObject trackedItem = subItem as ITrackableObject;
                            if (trackedItem != null)
                            {
                                WhenChanged(trackedItem, onChangedDelegate, cache);
                            }
                        }
                    }
                }
            }
        }


        public static bool IsDeleted(this ITrackableObject source)
        {
            if (source == null)
                return false;

            TrackingState tracker = GetTracker(source);
            return IsDeleted(source, tracker, null);
        }

        public static bool IsDeleted(this ITrackableObject source, out IEnumerable<ITrackableObject> deletedItems)
        {
            deletedItems = new List<ITrackableObject>();
            if (source == null)
                return false;

            TrackingState tracker = GetTracker(source);
            
            return IsDeleted(source, tracker, (List<ITrackableObject>) deletedItems);
        }

        private static bool IsDeleted(ITrackableObject source, TrackingState tracker, List<ITrackableObject> deletedItems)
        {
            //If the parent object is deleted then no need to check children.
            if (tracker.Deleted)
            {
                deletedItems?.Add(tracker.TrackedObject);
                return true;
            }

            bool hasDeletes = false;
            foreach (var property in tracker.UnmodifiedState)
            {
                if (property.Key.PropertyType.IsGenericType && property.Key.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                {
                    System.Collections.IEnumerable list = (System.Collections.IEnumerable)property.Value;
                    foreach (object item in list)
                    {
                        ITrackableObject trackedItem = item as ITrackableObject;
                        if (trackedItem != null)
                        {
                            if (IsDeleted(trackedItem, GetTracker(trackedItem), deletedItems))
                            {
                                if(deletedItems == null)
                                    return true;
                                hasDeletes = true;
                            }                                
                        }
                    }
                }
            }
            return hasDeletes;
        }

        public static bool IsNew(this ITrackableObject source)
        {
            if (source == null)
                return false;

            TrackingState tracker = GetTracker(source);
            return IsNew(source, tracker);
        }
        private static bool IsNew(ITrackableObject source, TrackingState tracker)
        {
            //If the parent object is added then no need to check children.
            if (tracker.Added)
                return true;

            foreach (var property in tracker.UnmodifiedState)
            {
                if (property.Key.PropertyType.IsGenericType && property.Key.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                {
                    System.Collections.IEnumerable list = (System.Collections.IEnumerable)property.Value;
                    foreach (object item in list)
                    {
                        ITrackableObject trackedItem = item as ITrackableObject;
                        if (trackedItem != null)
                        {
                            if (IsNew(trackedItem))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsModified(this ITrackableObject source, bool includeAddDelete = false)
        {
            if(source == null) 
                return false;

            TrackingState tracker = GetTracker(source);
            return IsModified(source, tracker, includeAddDelete);
        }

        private static bool IsModified(ITrackableObject source, TrackingState tracker, bool includeAddDelete = false)
        {
            //If the item has been flagged as modified return true even if no changes can be detected.
            if (tracker.Modified)
                return true;

            if (tracker.Added || tracker.Deleted)
                return includeAddDelete;

            foreach (var property in tracker.UnmodifiedState)
            {
                if (property.Key.PropertyType.IsGenericType && property.Key.PropertyType.IsAssignableTo(typeof(System.Collections.IEnumerable)))
                {
                    System.Collections.IEnumerable list = (System.Collections.IEnumerable)property.Value;
                    foreach (object item in list)
                    {
                        ITrackableObject trackedItem = item as ITrackableObject;
                        if (trackedItem != null)
                        {
                            if (IsModified(trackedItem, includeAddDelete))
                                return true;
                        }
                    }
                }
                else if (property.Key.PropertyType.IsAssignableTo(typeof(ITrackableObject)))
                {
                    ITrackableObject trackedItem = property.Key.GetValue(source) as ITrackableObject;
                    if (trackedItem != null)
                    {
                        if (IsModified(trackedItem, includeAddDelete))
                            return true;
                    }
                }
                else
                {
                    if (IsPropertyModified(source, property.Key, property.Value))
                        return true;
                }

            }
            return false;
        }

        private static bool IsPropertyModified(ITrackableObject source, PropertyInfo property, object referenceValue)
        {
            if (source == null)
                return false;

            if (referenceValue == null)
            {
                if (property.GetValue(source) != null)
                    return true;
            }
            else
            {
                var propertyValue = property.GetValue(source);
                if (!referenceValue.Equals(propertyValue))
                    return true;
            }

            return false;
        }

        public static bool HasChanges(this ITrackableObject source)
        {
            if (source == null)
                return false;

            TrackingState tracker = GetTracker(source);

            if (IsNew(source, tracker))
                return true;

            if (IsDeleted(source, tracker,null))
                return true;

            return IsModified(source, tracker);
        }

        #region Printing Entities

        /// <summary>
        /// Indicates that this item should be printed as part of a batch of entities.
        /// </summary>
        /// <param name="source">The entity item that needs to be printed.</param>
        public static void Print(this ITrackableObject source)
        {
            var tracker = GetTracker(source);
            tracker.Print = true;
        }

        /// <summary>
        /// Clear the print flag from the item.
        /// </summary>
        /// <param name="source">The tracked item to clear the print flag on.</param>
        public static void ClearPrint(this ITrackableObject source)
        {
            var tracker = GetTracker(source);
            tracker.Print = false;
        }

        /// <summary>
        /// Determines whether this entity requires printing.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if the specified source has print; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrintRequired(this ITrackableObject source)
        {
            if (source == null)
                return false;

            TrackingState tracker = GetTracker(source);
            return tracker.Print;
        }


        /// <summary>
        /// Determines whether any entities in the collection require printing.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if the specified source has print; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrintRequired(this IEnumerable<ITrackableObject> source)
        {
            if (source == null)
                return false;
            return source.Any(item => item.IsPrintRequired());
        }

        #endregion
    }
}
