using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public partial class RichTextEditor: InputTextArea, IAsyncDisposable
    {
        [Inject]
        public IJSRuntime JavaScript { get; set; }

        string _id;

        [Parameter]
        public string Id
        {
            get => _id ?? $"CKEditor_{uid}";
            set => _id = value;
        }

        readonly string uid = Guid.NewGuid().ToString().ToLower().Replace("-", "");

        private Task<IJSObjectReference> _ckEditorModule;
        private Task<IJSObjectReference> ckEditorModule => _ckEditorModule ??= JavaScript.InvokeAsync<IJSObjectReference>("import", "/_content/Oarw.Data.Tracking.Blazor/js/ckEditor/ckEditor-module.js?1").AsTask();

        private IJSObjectReference editor = null;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                editor = await (await ckEditorModule).InvokeAsync<IJSObjectReference>("initialiseCKEditor", Id, DotNetObjectReference.Create(this));
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        [JSInvokable]
        public Task EditorDataChanged(string data)
        {
           CurrentValue = data;
            StateHasChanged();
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                var module = await ckEditorModule;
                if (module != null && editor != null)
                {
                    await module.InvokeAsync<IJSObjectReference>("disposeCKEditor", editor);
                }
            }
            catch (Exception ex)
            {
            }            
        }
    }
}
