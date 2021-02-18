using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public interface ITrackedPrintService
    {
        string SelectedProfileId { get; set; }

        IEnumerable<ITrackedPrintProfile> Profiles { get; set; }

        void Print(IEnumerable<ITrackableObject> itemsToPrint);
    }
}
