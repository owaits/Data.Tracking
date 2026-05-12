using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using System;

namespace Oarw.Data.Tracking.Blazor.Prompt
{
    public partial class UserPrompt
    {
        private ModalJsInterop modalJs;

        public Guid Id { get; set; } = Guid.NewGuid();

        [Parameter, EditorRequired]
        public string Title { get; set; }


        public enum Levels
        {
            Primary,
            Danger
        }



        [Parameter]
        public Levels Level { get; set; } = Levels.Primary;

        public enum Sizes
        {
            Small,
            Normal,
            Large,
            ExtraLarge
        }

        [Parameter]
        public Sizes Size { get; set; } = Sizes.Normal;

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public RenderFragment Header { get; set; }

        [Parameter]
        public RenderFragment<UserPrompt> Footer { get; set; }

        [Parameter]
        public RenderFragment<UserPrompt> FooterTools { get; set; }

        [Parameter]
        public Func<Task> afterCancel { get; set; }

        [Parameter]
        public Func<Task> afterOpen { get; set; }

        [Parameter]
        public Func<Task<bool>> afterOk { get; set; }

        [Parameter]
        public Func<bool, Task> afterClose { get; set; }

        [CascadingParameter]
        public UserPromptProvider PromptProvider { get; set; }

        public event EventHandler OnRefresh;

        protected override async Task OnInitializedAsync()
        {
            if(js != null)
            {
                modalJs = new ModalJsInterop(js);
            }
            await Task.CompletedTask;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (OnRefresh != null)
                OnRefresh(this, EventArgs.Empty);
        }

        public void Refresh()
        {
            StateHasChanged();
        }

        protected string GetModalStyles()
        {
            string style = "";

            switch (Size)
            {
                case Sizes.Small:
                    style += " modal-sm";
                    break;
                case Sizes.Large:
                    style += " modal-lg";
                    break;
                case Sizes.ExtraLarge:
                    style += " modal-xl";
                    break;
            }

            return style;
        }

        public async Task Show(Func<Task<bool>> okCallback)
        {
            afterOk = okCallback;
            await Show();
        }

        public async Task Show()
        {
            if (PromptProvider != null)
            {
                PromptProvider.Show(this);
            }
            else
            {
                await modalJs.Open($"#{Id}");
                await Open();               
            }
        }

        public async Task<bool> Ok()
        {
            if (afterOk != null)
            {
                if (!await afterOk())
                    return false;
                afterOk = null;
            }

            if (PromptProvider == null)
            {
                await modalJs.Close($"#{Id}");
                await Close(true);
            }

            return true;
        }

        public async Task Cancel()
        {
            if (afterCancel != null)
            {
                await afterCancel();
                afterCancel = null;
            }

            if (PromptProvider == null)
            {
                await modalJs.Close($"#{Id}");
                await Close(false);
            }
        }

        public async Task Open()
        {
            if (afterOpen != null)
            {
                //Delay to allow time for the prompt to open.
                await Task.Delay(150);

                //Raise the open event.
                await afterOpen();
            }
        }

        public async Task Close(bool success)
        {
            if (afterClose != null)
            {
                await afterClose(success);
            }
        }
    }
}
