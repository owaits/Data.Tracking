using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public class TrackedUpdateBinding<TItem>: TrackedUpdateItem<TItem> where TItem : class, ITrackableObject
    {
    }
}
