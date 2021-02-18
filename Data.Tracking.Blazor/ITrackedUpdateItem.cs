using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public interface ITrackedUpdateItem
    {
        bool HasChanges();

        bool IsPrintRequired();

        Task CancelUpdate();

        Task ConfirmUpdate();

        Task ConfirmPrint();
    }
}
