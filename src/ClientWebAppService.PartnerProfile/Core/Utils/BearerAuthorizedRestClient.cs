using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using GL.MSA.Core.ResiliencyPolicy;
using GL.MSA.ISC.Transport.RestClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ClientWebAppService.PartnerProfile.Core
{
    /// <summary>
    /// Rest client enhanced with bearer authorization header 
    /// </summary>
    /// 
    [ExcludeFromCodeCoverage]
    public class BearerAuthorizedRestClient : RestClient, IRestClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerAuthorizedRestClient(HttpClient httpClient, IResiliencyPolicyProvider policyProvider, IHttpContextAccessor httpContextAccessor) : base(httpClient, policyProvider)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public new async Task<HttpResponseMessage> GetAsync(string url, IList<KeyValuePair<string, string>> requestHeader,
            PolicyEnum? policyEnum = null, bool cachedResult = true)
        {
            var bearer = EnsureAndGetAuthToken();
            return await base.GetAsync(url, new List<KeyValuePair<string, string>> { new("Authorization", bearer) }, policyEnum, cachedResult);
        }

        public new async Task<HttpResponseMessage> PostAsync(string url, IList<KeyValuePair<string, string>> requestHeader, HttpContent httpContent, TimeSpan? timeout = null)
        {
            var bearer = EnsureAndGetAuthToken();
            return await base.PostAsync(url, new List<KeyValuePair<string, string>> { new("Authorization", bearer) }, httpContent, timeout);
        }

        public new async Task<HttpResponseMessage> PutAsync(string url, IList<KeyValuePair<string, string>> requestHeader, HttpContent httpContent, PolicyEnum? policyEnum = null)
        {
            var bearer = EnsureAndGetAuthToken();

            return await base.PutAsync(url, new List<KeyValuePair<string, string>> { new("Authorization", bearer) }, httpContent, policyEnum);
        }

        private StringValues? EnsureAndGetAuthToken()
        {
            var bearer = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(bearer))
            {
                throw new InvalidOperationException("HttpContext contains no authorization header");
            }

            return bearer;
        }
    }
}
