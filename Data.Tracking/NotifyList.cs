using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking
{
    /// <summary>
    /// A generic list that subscribes to the property changed notification for all elements in the list and routing them to the lists change notification.
    /// </summary>
    /// <remarks>
    /// This allows you to listen to property notifications on all elements added to the list.
    /// </remarks>
    /// <typeparam name="TItem">The item to be stored in the list, must implement INotifyPropertyChanged.</typeparam>
    public class NotifyList<TItem> : IList<TItem>, INotifyPropertyChanged where TItem : INotifyPropertyChanged
    {
        private List<TItem> list { get; set; }= new List<TItem>();

        #region IList

        public TItem this[int index]
        {
            get => list[index];
            set
            {
                if (index >= 0 && index < list.Count)
                    list[index].PropertyChanged -= Item_PropertyChanged;
                value.PropertyChanged += Item_PropertyChanged;
                list[index] = value;
            }
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(TItem item)
        {
            list.Add(item);
            item.PropertyChanged += Item_PropertyChanged;
        }

        public void AddRange(IEnumerable<TItem> items)
        {
            list.AddRange(items);

            foreach (var item in list)
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public void Clear()
        {
            foreach (var item in list)
                item.PropertyChanged -= Item_PropertyChanged;
            list.Clear();
        }

        public bool Contains(TItem item)
        {
            return list.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(TItem item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, TItem item)
        {
            list.Insert(index, item);
            item.PropertyChanged += Item_PropertyChanged;
        }

        public bool Remove(TItem item)
        {
            item.PropertyChanged -= Item_PropertyChanged;
            return list.Remove(item);
        }


        public void RemoveAt(int index)
        {
            this[index].PropertyChanged -= Item_PropertyChanged;
            list.RemoveAt(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)list).GetEnumerator();
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, e);
        }

        #endregion

    }
}
