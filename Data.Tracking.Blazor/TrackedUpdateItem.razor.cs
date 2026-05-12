using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private Action changeHandler = null;

        [Inject]
        public HttpClient Http { get; set; }

        [Inject]
        public IServiceProvider Services { get; set; }

        [Inject]
        public ILogger<TrackedUpdateItem<TItem>> Log { get; set; }

        public ITrackedPrintService print { get; set;  }

        [CascadingParameter]
        public TrackedUpdate UpdateContainer { get; set; }

        /// <summary>
        /// Gets or sets the edit items that are mopnitored for changes. This can be set directly or by setting the EditItem property. If both are set, EditItems takes precedence.
        /// </summary>
        [Parameter]
        public IEnumerable<ITrackableObject> EditItems { get; set; }

        /// <summary>
        /// Gets or sets the edit items that are mopnitored for changes. This can be set directly or by setting the EditItem property. If both are set, EditItems takes precedence.
        /// </summary>
        [Parameter]
        public ITrackableObject EditItem { get; set; }

        [Parameter,EditorRequired]
        public string Url { get; set; }

        [Parameter]
        public Action BeforeUpdate { get; set; }

        [Parameter]
        public Action afterUpdate { get; set; }

        [Parameter]
        public RenderFragment<TItem> EditTemplate { get; set; }


        protected override void OnInitialized()
        {
            changeHandler = new Action(() => UpdateContainer.Refresh());

            print = Services.GetService<ITrackedPrintService>();

            if (UpdateContainer != null)
                UpdateContainer.Updates.Add(this);
        }

        private IEnumerable<ITrackableObject> editItems = null;
        private ITrackableObject editItem = null;

        override protected void OnParametersSet()
        {
            if (editItem != EditItem)
            {
                editItem = EditItem;
                EditItems = editItem != null ? new List<ITrackableObject>() { editItem } : null;
            }

            if (editItems != EditItems)
            {
                editItems = EditItems;
                if (changeHandler != null)
                    editItems.WhenChanged(changeHandler);
            }
        }

        public bool TryGetEditTemplate(ITrackableObject item, out RenderFragment editTemplate)
        {
            if(EditTemplate == null)
            {
                editTemplate = null;
                return false;
            }

            editTemplate = EditTemplate((TItem)item);
            return true;
        }

        public bool CanUpdate(ITrackableObject item)
        {
            return typeof(TItem) == item.GetType();
        }

        public bool HasChanges()
        {
            return editItems != null && editItems.IsTracking() && editItems.HasChanges();
        }

        public bool IsPrintRequired()
        {
            return editItems != null && editItems.IsTracking() && editItems.IsPrintRequired();
        }

        public async Task<bool> Create(IEnumerable<ITrackableObject> items)
        {
            var response = await Http.PostAsJsonAsync(Url, items);

            if (!response.IsSuccessStatusCode)
                return false;

            return true;
        }

        public async Task<bool> Update(IEnumerable<ITrackableObject> items)
        {
            if (items.Any(item => item.IsModified(true)))
            {
                //Put the changes to the edit item on the server.
                var response = await Http.PutAsJsonAsync(Url, items);

                if (!response.IsSuccessStatusCode)
                    return false;

                //Delete any child items that require deletion.
                foreach (var parentItem in items)
                { 
                    if (parentItem.IsDeleted(out IEnumerable<ITrackableObject> subDeleteItems))
                    {
                        foreach (var item in subDeleteItems)
                        {
                            await Http.DeleteAsync(Url + $"/{item.Id}");
                        }
                    }

                    //After we have saved these changes to the server, start tracking again to reset changes.
                    parentItem.StartTracking();
                }                
            }

            return true;
        }

        public async Task CancelUpdate()
        {
            if(EditItems!= null) EditItems.Undo();
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<TrackedUpdateError>> ConfirmUpdate()
        {
            List<TrackedUpdateError> errors = new List<TrackedUpdateError>();
            try
            {
                if (BeforeUpdate != null)
                {
                    BeforeUpdate();
                }

                IEnumerable<ITrackableObject> additions = editItems.Where(item => item.IsNew()).ToList();
                if (additions.Any())
                {
                    var response = await Http.PostAsJsonAsync(Url, additions.Cast<TItem>());

                    if(!response.IsSuccessStatusCode)
                    {
                        errors.Add(new TrackedUpdateError()
                        {
                            Title = "Failed to Create",
                            Message = await response.Content.ReadAsStringAsync()
                        });                       
                    }

                    additions.StartTracking();
                }

                ICollection<ITrackableObject> updates = editItems.Where(item => item.IsModified()).ToList();

                if (updates.Any())
                {
                    var response = await Http.PutAsJsonAsync(Url, updates.Cast<TItem>());
                    if (!response.IsSuccessStatusCode)
                    {
                        errors.Add(new TrackedUpdateError()
                        {
                            Title = "Failed to Update",
                            Message = await response.Content.ReadAsStringAsync()
                        });
                    }

                    updates.StartTracking();
                }

                IEnumerable<ITrackableObject> deletes = editItems.Where(item => item.IsDeleted()).ToList();
                if (deletes.Any())
                {
                    foreach (var itemToDelete in deletes)
                    {
                        if (itemToDelete.IsDeleted(out IEnumerable<ITrackableObject> subDeleteItems))
                        {
                            foreach (var item in subDeleteItems)
                            {
                                var response = await Http.DeleteAsync(Url + $"/{item.Id}");
                                if (!response.IsSuccessStatusCode)
                                {
                                    errors.Add(new TrackedUpdateError()
                                    {
                                        Title = "Failed to Delete",
                                        Message = await response.Content.ReadAsStringAsync()
                                    });
                                }
                            }                                
                        }
                    }

                    deletes.StartTracking();
                }

                if (afterUpdate != null)
                {
                    afterUpdate();
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Error updating tracked items");

                errors.Add(new TrackedUpdateError()
                {
                    Title = "Error",
                    Message = ex.Message
                });
            }

            return errors;
        }

        public async Task<IEnumerable<TrackedUpdateError>> ConfirmPrint()
        {
            List<TrackedUpdateError> errors = new List<TrackedUpdateError>();

            try
            {
                var itemsToPrint = editItems.Where(item => item.IsPrintRequired());
                print.Print(itemsToPrint);

                foreach (var item in editItems)
                    item.ClearPrint();

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Error printing tracked items");

                errors.Add(new TrackedUpdateError()
                {
                    Title = "Error",
                    Message = ex.Message
                });
            }

            return errors;
        }
    }
}
