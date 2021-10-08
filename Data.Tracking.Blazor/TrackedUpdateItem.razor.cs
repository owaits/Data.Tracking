using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public partial class TrackedUpdateItem<TItem>
    {
        [Inject]
        public HttpClient Http { get; set; }

        [Inject]
        public NavigationManager Url { get; set; }

        [Inject]
        public ITrackedPrintService print { get; set;  }

        [CascadingParameter]
        public TrackedUpdate UpdateContainer { get; set; }

        private string errorMessage { get; set; }

        private string selectedPrintProfile { get; set; }

        [Parameter]
        public IEnumerable<ITrackableObject> editItems { get; set; }

        [Parameter]
        public ITrackableObject editItem
        {
            get { return editItems?.FirstOrDefault(); }
            set { editItems = value == null ? null : new ITrackableObject[] { value }; }
        }

        [Parameter]
        public string url { get; set; }

        [Parameter]
        public Action afterUpdate { get; set; }

        [Parameter]
        public RenderFragment<TItem> EditContent { get; set; }


        protected override void OnInitialized()
        {
            if (UpdateContainer != null)
                UpdateContainer.Updates.Add(this);
        }

        public bool HasChanges()
        {
            return editItems != null && editItems.IsTracking() && editItems.HasChanges();
        }

        public bool IsPrintRequired()
        {
            return editItems != null && editItems.IsTracking() && editItems.IsPrintRequired();
        }

        public async Task CancelUpdate()
        {
            editItems.Undo();
            await Task.CompletedTask;
        }

        public async Task ConfirmUpdate()
        {

            try
            {
                IEnumerable<ITrackableObject> additions = editItems.Where(item => item.IsNew()).ToList();
                if (additions.Any())
                {
                    await Http.PostAsJsonAsync(url, additions.Cast<TItem>());
                    additions.StartTracking();
                }
                else
                {
                    Console.WriteLine("Nothing to Add");
                }

                ICollection<ITrackableObject> updates = editItems.Where(item => item.IsModified()).ToList();

                if (updates.Any())
                {
                    await Http.PutAsJsonAsync(url, updates.Cast<TItem>());
                    updates.StartTracking();
                }
                else
                {
                    Console.WriteLine("Nothing to Update");
                }

                IEnumerable<ITrackableObject> deletes = editItems.Where(item => item.IsDeleted()).ToList();
                if (deletes.Any())
                {
                    foreach (var itemToDelete in deletes)
                    {
                        if (itemToDelete.IsDeleted(out IEnumerable<ITrackableObject> subDeleteItems))
                        {
                            foreach (var item in subDeleteItems)
                                await Http.DeleteAsync(url + $"/{item.Id}");
                        }
                    }

                    deletes.StartTracking();
                }
                else
                {
                    Console.WriteLine("Nothing to Delete");
                }

                if (afterUpdate != null)
                {
                    afterUpdate();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public async Task ConfirmPrint()
        {
            var itemsToPrint = editItems.Where(item => item.IsPrintRequired());
            print.Print(itemsToPrint);

            foreach (var item in editItems)
                item.ClearPrint();

            await Task.CompletedTask;
        }
    }
}
