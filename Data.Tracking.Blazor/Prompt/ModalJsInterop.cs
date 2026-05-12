using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor.Prompt
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class ModalJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public ModalJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Oarw.Data.Tracking.Blazor/js/modal.js").AsTask());
        }

        public async Task Open(string modalId)
        {
            var module = await moduleTask.Value;
            await module.InvokeAsync<string>("openModal", modalId);
        }

        public async Task Close(string modalId)
        {
            var module = await moduleTask.Value;
            await module.InvokeAsync<string>("closeModal", modalId);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}