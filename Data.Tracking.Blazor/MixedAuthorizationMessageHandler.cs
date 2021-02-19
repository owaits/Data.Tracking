using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Oarw.Data.Tracking.Blazor
{
    /// <summary>
    /// This access token provider allows for both secure and unsecure HTTP calls to an API.
    /// </summary>
    /// <remarks>
    /// This allows you to use a single HTTP client to make both secure and unsecure API calls from a blazor app.
    /// </remarks>
    public class MixedAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _provider;
        private readonly NavigationManager _navigation;
        private AccessToken _lastToken;
        private AuthenticationHeaderValue _cachedHeader;
        private Uri[] _authorizedUris;
        private AccessTokenRequestOptions _tokenOptions;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthorizationMessageHandler"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IAccessTokenProvider"/> to use for provisioning tokens.</param>
        /// <param name="navigation">The <see cref="NavigationManager"/> to use for performing redirections.</param>
        public MixedAuthorizationMessageHandler(
            IAccessTokenProvider provider,
            NavigationManager navigation)
        {
            _provider = provider;
            _navigation = navigation;

            ConfigureHandler(new[] { navigation.BaseUri });
        }

        /// <summary>
        /// Gets or sets whether anonymous requests with no token in the header are permitted.
        /// </summary>
        /// <remarks>
        /// When set to false if the user is not logged in and an HTTP request is made an AccessTokenNotAvailableException will be thrown. When true the request
        /// will be allowed with no token added to the header.
        /// </remarks>
        public bool AllowAnonymous { get; set; } = true;

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;
            if (_authorizedUris == null)
            {
                throw new InvalidOperationException($"The '{nameof(AuthorizationMessageHandler)}' is not configured. " +
                    $"Call '{nameof(AuthorizationMessageHandler.ConfigureHandler)}' and provide a list of endpoint urls to attach the token to.");
            }

            if (_authorizedUris.Any(uri => uri.IsBaseOf(request.RequestUri)))
            {
                if (_lastToken == null || now >= _lastToken.Expires.AddMinutes(-5))
                {
                    var tokenResult = _tokenOptions != null ?
                        await _provider.RequestAccessToken(_tokenOptions) :
                        await _provider.RequestAccessToken();

                    if (tokenResult.TryGetToken(out var token))
                    {
                        _lastToken = token;
                        _cachedHeader = new AuthenticationHeaderValue("Bearer", _lastToken.Value);
                    }
                    else
                    {
                        if (!AllowAnonymous)
                            throw new AccessTokenNotAvailableException(_navigation, tokenResult, _tokenOptions?.Scopes);
                    }
                }

                // We don't try to handle 401s and retry the request with a new token automatically since that would mean we need to copy the request
                // headers and buffer the body and we expect that the user instead handles the 401s. (Also, we can't really handle all 401s as we might
                // not be able to provision a token without user interaction).
                request.Headers.Authorization = _cachedHeader;
            }

            var result = await base.SendAsync(request, cancellationToken);

            //If we get forbidden result we may need to request the access token again, this can happen when roles have been added to the user.
            if (result.StatusCode == System.Net.HttpStatusCode.Forbidden)
                _lastToken = null;

            return result;
        }

        /// <summary>
        /// Configures this handler to authorize outbound HTTP requests using an access token. The access token is only attached if at least one of
        /// <paramref name="authorizedUrls" /> is a base of <see cref="HttpRequestMessage.RequestUri" />.
        /// </summary>
        /// <param name="authorizedUrls">The base addresses of endpoint URLs to which the token will be attached.</param>
        /// <param name="scopes">The list of scopes to use when requesting an access token.</param>
        /// <param name="returnUrl">The return URL to use in case there is an issue provisioning the token and a redirection to the
        /// identity provider is necessary.
        /// </param>
        /// <returns>This <see cref="AuthorizationMessageHandler"/>.</returns>
        public MixedAuthorizationMessageHandler ConfigureHandler(
            IEnumerable<string> authorizedUrls,
            IEnumerable<string> scopes = null,
            string returnUrl = null)
        {
            if (_authorizedUris != null)
            {
                throw new InvalidOperationException("Handler already configured.");
            }

            if (authorizedUrls == null)
            {
                throw new ArgumentNullException(nameof(authorizedUrls));
            }

            var uris = authorizedUrls.Select(uri => new Uri(uri, UriKind.Absolute)).ToArray();
            if (uris.Length == 0)
            {
                throw new ArgumentException("At least one URL must be configured.", nameof(authorizedUrls));
            }

            _authorizedUris = uris;
            var scopesList = scopes?.ToArray();
            if (scopesList != null || returnUrl != null)
            {
                _tokenOptions = new AccessTokenRequestOptions
                {
                    Scopes = scopesList,
                    ReturnUrl = returnUrl
                };
            }

            return this;
        }
    }

    public static class MixedAuthorizationMessageHandlerExtensions
    {
    
        public static IHttpClientBuilder AddMixedAuthorizationMessageHandler(this IHttpClientBuilder builder, bool allowAnonymousRequests)
        {
            builder.Services.AddScoped<MixedAuthorizationMessageHandler>(sp => new MixedAuthorizationMessageHandler(sp.GetService<IAccessTokenProvider>(), sp.GetService<NavigationManager>()) { AllowAnonymous = allowAnonymousRequests});
            return builder.AddHttpMessageHandler<MixedAuthorizationMessageHandler>();
        }
    }

}
