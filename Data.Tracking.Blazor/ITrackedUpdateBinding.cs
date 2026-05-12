using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public interface ITrackedUpdateBinding
    {
        bool CanUpdate(ITrackableObject item);

        bool HasChanges();

        bool IsPrintRequired();

        bool TryGetEditTemplate(ITrackableObject item, out RenderFragment editTemplate);

        Task<bool> Create(IEnumerable<ITrackableObject> items);

        Task<bool> Update(IEnumerable<ITrackableObject> items);

        Task CancelUpdate();

        Task<IEnumerable<TrackedUpdateError>> ConfirmUpdate();

        Task<IEnumerable<TrackedUpdateError>> ConfirmPrint();
    }
}
