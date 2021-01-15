using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Data.Tracking
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
                    if (OnChanged != null)
                        OnChanged();
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
                    if (OnChanged != null)
                        OnChanged();
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
                    if (OnChanged != null)
                        OnChanged();
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
                    if (OnChanged != null)
                        OnChanged();
                }
            }
        }

        private Dictionary<PropertyInfo, object> unmodifiedState = new Dictionary<PropertyInfo, object>();

        public Dictionary<PropertyInfo, object> UnmodifiedState { get => unmodifiedState; set => unmodifiedState = value; }

        /// <summary>
        /// Gets or sets a delegate to call when the object being tracked changes.
        /// </summary>
        public Action OnChanged {get;set;}

        public void Dispose()
        {
            TrackedObject = null;
        }
    }
}
