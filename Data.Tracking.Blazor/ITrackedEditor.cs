using Oarw.Data.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Tracking.Blazor
{
    public interface ITrackedEditor
    {
        public Guid EditorId { get; set; }

        void StartCreate(ITrackableObject editItem);

        void StartEdit(ITrackableObject editItem);
    }
}
