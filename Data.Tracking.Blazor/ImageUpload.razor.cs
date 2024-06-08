using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public partial class ImageUpload :InputBase<string>
    {
        [Inject]
        public HttpClient Http { get; set; }

        [Parameter]
        public Guid LinkId { get; set; }

        [Parameter]
        public Func<IBrowserFile, Task<(string url, object state)>> BeginUpload { get; set; }

        [Parameter]
        public Func<IBrowserFile, object, Task> EndUpload { get; set; }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
        }

        protected override bool TryParseValueFromString(string value, out string result, [NotNullWhen(false)] out string validationErrorMessage)
        {
            result = value ?? "";
            validationErrorMessage = null;
            return true;
        }
    }
}
