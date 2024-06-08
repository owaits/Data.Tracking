using Microsoft.AspNetCore.Components;
using System;

namespace Oarw.Data.Tracking.Blazor.Prompt
{
    public partial class Thumbnail
    {
        private UserPrompt viewer;

        [Parameter]
        public string Url { get; set; }

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

        protected void ShowViewer()
        {
            if(viewer != null)
                viewer.Show();

            Console.WriteLine("click");
        }
    }
}
