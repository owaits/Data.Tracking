using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public interface ITrackedEditor
    {
        public Guid EditorId { get; set; }

        Task StartCreate(ITrackableObject editItem);

        Task StartEdit(ITrackableObject editItem);

        Task<bool> SaveEdit();
    }
}
