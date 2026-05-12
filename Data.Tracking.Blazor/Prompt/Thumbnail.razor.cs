using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor.Prompt
{
    public partial class Thumbnail
    {
        private UserPrompt viewer;

        [Parameter, EditorRequired]
        public string Url { get; set; }

        [Parameter, EditorRequired]
        public string Title { get; set; }

        [Parameter]
        public string Description { get; set; }

        [Parameter]
        public string Class { get; set; }

        private string url = null;

        protected override void OnParametersSet()
        {
            if(Url != url) 
            {
                StateHasChanged();
                url = Url;
            }
        }

        protected async Task ShowViewer()
        {
            if(viewer != null)
                await viewer.Show();
        }
    }
}
