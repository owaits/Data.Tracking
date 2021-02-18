using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    public class HttpSecure
    {
        private readonly HttpClient http;
        private readonly NavigationManager uriHelper;
        private readonly IAccessTokenProvider accessTokenProvider;

        public HttpSecure(HttpClient http, IAccessTokenProvider accessTokenProvider, NavigationManager uri)
        {
            this.http = http;
            this.uriHelper = uri;
            this.accessTokenProvider = accessTokenProvider;
        }

        public async Task NavigateToSecure(string url)
        {
            var accessToken = await accessTokenProvider.RequestAccessToken();
            if(accessToken.TryGetToken(out var token))
            {
                url += (url.Contains("?") ? "&" : "?");
                url += $"token={token.Value}";
            }

            uriHelper.NavigateTo(url, true);
        }
    }
}
