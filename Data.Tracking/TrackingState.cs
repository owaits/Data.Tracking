﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Oarw.Data.Tracking
{
    public class TrackingState:IDisposable
    {
        public TrackingState(ITrackableObject trackedObject)
        {
            TrackedObject = trackedObject;
        }

        private ITrackableObject trackedObject = null;

        public ITrackableObject TrackedObject
        {
            get
            {
                return trackedObject;
            }
            set
            {
                if (trackedObject != value)
                {
                    if (trackedObject is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)trackedObject).PropertyChanged -= TrackingState_PropertyChanged;
                    }

                    trackedObject = value;
                    if (trackedObject is INotifyPropertyChanged)
                    {
                        ((INotifyPropertyChanged)trackedObject).PropertyChanged += TrackingState_PropertyChanged;
                    }
                }
            }
        }

        private void TrackingState_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (object.ReferenceEquals(sender, trackedObject))
            {
                var unmodifiedValue = UnmodifiedState.Where(item => item.Key.Name == e.PropertyName);
                if (unmodifiedValue != null)
                {
                    foreach (var subscriber in OnChanged)
                        subscriber();
                }
            }
        }

        private bool deleted = false;

        /// <summary>
        /// Gets or sets whether the object being tracked has been deleted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if deleted; otherwise, <c>false</c>.
        /// </value>
        public bool Deleted
        {
            get { return deleted;  }
            set
            {
                if(deleted != value)
                {
                    deleted = value;
                    foreach (var subscriber in OnChanged)
                        subscriber();
                }
            }
        }

        private bool added = false;

        /// <summary>
        /// Gets or sets whether the object being tracked has been added.
        /// </summary>
        /// <value>
        ///   <c>true</c> if added; otherwise, <c>false</c>.
        /// </value>
        public bool Added
        {
            get { return added; }
            set
            {
                if (added != value)
                {
                    added = value;
                    foreach (var subscriber in OnChanged)
                        subscriber();
                }
            }
        }

        private bool modified = false;

        /// <summary>
        /// Gets or sets whether the object being tracked has been modified. This is used along with change tracking to determine if the item has been modified.
        /// The user can use this flag to force the entity to be modified even if no changes are detected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if added; otherwise, <c>false</c>.
        /// </value>
        public bool Modified
        {
            get { return modified; }
            set
            {
                if (modified != value)
                {
                    modified = value;
                    foreach (var subscriber in OnChanged)
                        subscriber();
                }
            }
        }

        private bool print = false;

        /// <summary>
        /// Gets or sets a value indicating whether the item being tracked is selected for printing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if print; otherwise, <c>false</c>.
        /// </value>
        public bool Print
        {
            get { return print; }
            set
            {
                if (print != value)
                {
                    print = value;
                    foreach (var subscriber in OnChanged)
                        subscriber();
                }
            }
        }

        private Dictionary<PropertyInfo, object> unmodifiedState = new Dictionary<PropertyInfo, object>();

        public Dictionary<PropertyInfo, object> UnmodifiedState { get => unmodifiedState; set => unmodifiedState = value; }

        /// <summary>
        /// Gets or sets a delegate to call when the object being tracked changes.
        /// </summary>
        public HashSet<Action> OnChanged {get;set;} = new HashSet<Action>();

        public void Dispose()
        {
            TrackedObject = null;
        }
    }
}
