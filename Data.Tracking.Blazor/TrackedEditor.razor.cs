using Oarw.Data.Tracking;
using System.Net.Http;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Oarw.Data.Tracking.Blazor
{
    public partial class TrackedEditor<TItem>: ITrackedEditor where TItem: class, ITrackableObject
    {
        public Guid EditorId { get; set; } = Guid.NewGuid();

        private bool Create { get; set; }

        public TItem EditItem { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public string url { get; set; }

        [Parameter]
        public string ModalCSS { get; set; }

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

        public void StartEdit(ITrackableObject editItem)
        {
            EditItem = (TItem) editItem;

            if (OnStartEdit != null)
                OnStartEdit(Create, editItem);

            StateHasChanged();
        }

        public void StartCreate(ITrackableObject editItem)
        {
            Create = true;

            StartEdit(editItem);
        }

        public void CancelEdit()
        {
            EditItem = null;
            Create = false;

            JsRuntime.InvokeAsync<string>("closeModal", $"#trackedEditor_{EditorId}");

            if (OnCancelEdit != null)
                OnCancelEdit(Create, EditItem);
        }

        public async Task SaveEdit()
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
                    await Http.PostAsJsonAsync(url, itemToSave);
                }
                else
                {
                    if (itemToSave.IsModified())
                    {
                        //Put the changes to the edit item on the server.
                        await Http.PutAsJsonAsync(url, new[] { itemToSave });

                        //After we have saved these changes to the server, start tracking again to reset changes.
                        itemToSave.StartTracking();
                    }
                }
            }

            CancelEdit();

            if (OnFinishEdit != null)
                OnFinishEdit(isCreate, itemToSave);
        }
    }

}
