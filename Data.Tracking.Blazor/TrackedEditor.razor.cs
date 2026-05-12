using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oarw.Data.Tracking;
using Oarw.Data.Tracking.Blazor.Prompt;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static Oarw.Data.Tracking.Blazor.Prompt.UserPrompt;

namespace Oarw.Data.Tracking.Blazor
{
    public partial class TrackedEditor<TItem>: ITrackedEditor where TItem: class, ITrackableObject
    {
        private EditPrompt userPrompt;

        public Guid EditorId { get; set; } = Guid.NewGuid();

        private bool Create { get; set; }

        public TItem EditItem { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string url { get; set; }

        [Parameter]
        public Sizes Size { get; set; } = Sizes.Normal;

        [Parameter]
        public RenderFragment<TItem> EditModal { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public Action<bool, ITrackableObject> OnStartEdit { get; set; }

        [Parameter]
        public Action<bool, ITrackableObject> OnSaveEdit { get; set; }

        [Parameter]
        public Action<bool, ITrackableObject> OnFinishEdit { get; set; }

        [Parameter]
        public Action<bool, ITrackableObject> OnCancelEdit { get; set; }

        private HashSet<ITrackedUpdateItem> bindings = new HashSet<ITrackedUpdateItem>();

        public HashSet<ITrackedUpdateItem> Bindings
        {
            get { return bindings; }
        }

        public async Task StartEdit(ITrackableObject editItem)
        {
            EditItem = (TItem) editItem;

            if (OnStartEdit != null)
                OnStartEdit(Create, editItem);

            await userPrompt.Show(editItem, SaveEdit);
        }

        public async Task StartCreate(ITrackableObject editItem)
        {
            Create = true;

            await StartEdit(editItem);
        }

        public async Task CancelEdit()
        {
            ITrackableObject trackedEditItem = (ITrackableObject) EditItem;
            if(trackedEditItem.IsTracking())
            {
                trackedEditItem.Undo();
            }

            EditItem = null;
            Create = false;

            if (OnCancelEdit != null)
                OnCancelEdit(Create, trackedEditItem);
        }

        public async Task<bool> SaveEdit()
        {
            TItem itemToSave = EditItem;
            bool isCreate = Create;

            if(OnSaveEdit != null)
            {
                OnSaveEdit(isCreate, itemToSave);
            }
            else
            {
                if (isCreate)
                {
                    var response = await Http.PostAsJsonAsync(url, new[] { itemToSave });

                    if (!response.IsSuccessStatusCode)
                        return false;
                }
                else
                {
                    if (itemToSave.IsModified(true))
                    {
                        //Put the changes to the edit item on the server.
                        var response = await Http.PutAsJsonAsync(url, new[] { itemToSave });

                        if (!response.IsSuccessStatusCode)
                            return false;


                        //Delete any child items that require deletion.
                        if (itemToSave.IsDeleted(out IEnumerable<ITrackableObject> subDeleteItems))
                        {
                            foreach (var item in subDeleteItems)
                            {
                                await Http.DeleteAsync(url + $"/{item.Id}");
                            }                                
                        }

                        //After we have saved these changes to the server, start tracking again to reset changes.
                        itemToSave.StartTracking();
                    }
                }
            }

            await CancelEdit();

            if (OnFinishEdit != null)
                OnFinishEdit(isCreate, itemToSave);

            return true;
        }
    }

}
