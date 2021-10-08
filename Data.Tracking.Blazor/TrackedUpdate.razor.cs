using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public partial class TrackedUpdate : IAsyncDisposable
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public TimeSpan? AutoSaveInterval { get; set; }

        public DateTime? NextAutoSave { get; set; }

        private string errorMessage { get; set; }

        private string selectedPrintProfile { get; set; }

        private HashSet<ITrackedUpdateItem> updates = new HashSet<ITrackedUpdateItem>();

        public HashSet<ITrackedUpdateItem> Updates { get { return updates; } }

        private System.Timers.Timer autoSaveTimer;

        [Parameter]
        public Func<bool> beforeUpdate { get; set; }

        [Parameter]
        public Action afterUpdate { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (AutoSaveInterval != null)
            {
                autoSaveTimer = new System.Timers.Timer(500);
                autoSaveTimer.Elapsed += AutoSave;
                autoSaveTimer.Enabled = true;
            }
        }

        private void AutoSave(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (AutoSaveInterval != null)
            {
                if (NextAutoSave != null && HasChanges())
                {
                    if (((DateTime)NextAutoSave).Subtract(DateTime.Now).TotalSeconds < 0)
                    {
                        NextAutoSave = null;
                        Task.Run(() => ConfirmUpdate());
                    }
                    else
                    {
                        StateHasChanged();
                    }
                }
                else
                {
                    NextAutoSave = DateTime.Now.Add((TimeSpan)AutoSaveInterval);
                }
            }
        }

        public bool HasChanges()
        {
            return Updates.Any(item => item.HasChanges());
        }

        public bool IsPrintRequired()
        {
            return Updates.Any(item => item.IsPrintRequired());
        }

        public async Task CancelUpdate()
        {
            foreach (var item in Updates)
                await item.CancelUpdate();
        }

        public async Task ConfirmUpdate()
        {
            if (beforeUpdate != null)
            {
                beforeUpdate();
            }

            foreach (var item in Updates)
                await item.ConfirmUpdate();

            if (afterUpdate != null)
            {
                afterUpdate();
            }
        }

        protected async Task Print()
        {
            foreach (var item in Updates)
                await item.ConfirmPrint();

            if (afterUpdate != null)
            {
                afterUpdate();
            }
        }

        public async ValueTask DisposeAsync()
        {
            //If auto save is enabled and the user navigates away update the changes.
            if (AutoSaveInterval != null && HasChanges())
            {
                await ConfirmUpdate();
            }
        }
    }
}
