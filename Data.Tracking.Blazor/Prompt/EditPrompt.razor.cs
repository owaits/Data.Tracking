using System.Threading.Tasks;
using System;

namespace Oarw.Data.Tracking.Blazor.Prompt
{
    public partial class EditPrompt:UserPrompt
    {
        private object model = null;

        public object Model 
        {
            get { return model; }
            set
            {
                if(model != value)
                {
                    model = value;
                    StateHasChanged();
                }
            }
        }

        public async Task Show(object model, Func<Task<bool>> okCallback)
        {
            Model = model;
            await base.Show(okCallback);
        }

        public async Task Show(object model)
        {
            Model = model;
            await Show();
        }

        protected async Task Submit()
        {
            await Ok();
        }
    }
}
