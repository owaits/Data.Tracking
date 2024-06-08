using Microsoft.AspNetCore.Components;
using static System.Net.WebRequestMethods;
using System.Numerics;
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

        public void Show(object model, Func<Task<bool>> okCallback)
        {
            Model = model;
            base.Show(okCallback);
        }

        public void Show(object model)
        {
            Model = model;
            Show();
        }

        protected async Task Submit()
        {
            await Ok();
        }
    }
}
