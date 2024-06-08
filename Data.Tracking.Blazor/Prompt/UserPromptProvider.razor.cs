using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor.Prompt
{
    public partial class UserPromptProvider:IDisposable
    {
        private UserPrompt globalPrompt;

        private EditPrompt globalEditPrompt;

        [Inject]
        public NavigationManager Navigation { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected string Title { get; set; }

        public RenderFragment PromptContent { get; set; }

        public UserPrompt.Levels Level { get; set; } = UserPrompt.Levels.Primary;

        public UserPrompt.Sizes Size { get; set; } = UserPrompt.Sizes.Normal;

        public object Model { get; set; }

        public RenderFragment PromptHeader { get; set; }

        public RenderFragment<UserPrompt> PromptFooter { get; set; }

        public RenderFragment<UserPrompt> PromptFooterTools { get; set; }

        public Func<Task> afterCancel { get; set; }

        public Func<Task<bool>> afterOk { get; set; }

        private UserPrompt promptBinding;

        protected UserPrompt PromptBinding
        {
            get { return promptBinding; }
            set
            {
                if (promptBinding != value)
                {
                    if (promptBinding != null)
                    {
                        promptBinding.OnRefresh -= PromptBinding_OnRefresh;
                    }
                    promptBinding = value;
                    if (promptBinding != null)
                    {
                        promptBinding.OnRefresh += PromptBinding_OnRefresh;

                        Level = promptBinding.Level;
                        Size = promptBinding.Size;
                    }

                    Title = promptBinding?.Title;
                    PromptHeader = promptBinding?.Header;
                    PromptContent = promptBinding?.ChildContent;
                    PromptFooter = promptBinding?.Footer;
                    PromptFooterTools = promptBinding?.FooterTools;

                    Model = (promptBinding as EditPrompt)?.Model;
                }
            }
        }

        private IDisposable locationChangingRegistration = null;

        private async ValueTask LocationChanging(LocationChangingContext arg)
        {
            await globalEditPrompt.Cancel();
            await globalPrompt.Cancel();
            PromptBinding = null;
        }

        private void PromptBinding_OnRefresh(object sender, EventArgs e)
        {
            globalEditPrompt.Refresh();
            globalPrompt.Refresh();
        }

        public void Show(UserPrompt prompt)
        {
            PromptBinding = prompt;

            StateHasChanged();

            if(PromptBinding is EditPrompt)
            {
                globalEditPrompt.Model = ((EditPrompt)PromptBinding).Model;
                globalEditPrompt.Show();
            }
            else
            {
                globalPrompt.Show();
            }

            if(locationChangingRegistration == null)
            {
                locationChangingRegistration = Navigation.RegisterLocationChangingHandler(LocationChanging);
            }            
        }

        protected async Task Open()
        {
            if (PromptBinding != null)
            {
                await PromptBinding.Open();
            }
        }

        protected async Task  Close(bool success)
        {
            if (PromptBinding != null)
            {
                await PromptBinding.Close(success);
                PromptBinding = null;
            }

            if(locationChangingRegistration != null)
            {
                locationChangingRegistration.Dispose();
                locationChangingRegistration = null;
            }
        }

        public async Task<bool> Ok()
        {
            if (PromptBinding != null)
                return await PromptBinding.Ok();
            return true;
        }

        public async Task Cancel()
        {
            if (PromptBinding != null)
                await PromptBinding.Cancel();
        }

        public void Dispose()
        {
            if (locationChangingRegistration != null)
            {
                locationChangingRegistration.Dispose();
                locationChangingRegistration = null;
            }
        }
    }
}
