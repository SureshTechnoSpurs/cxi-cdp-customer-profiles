using ClientWebAppService.PartnerProfile.Core.Exceptions;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.Core.Utils
{
    ///<inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class RequestDispatcher : IRequestDispatcher
    {
        ///<inheritdoc/>
        public async Task<TResult> DispatchRequestResult<TResult>(Func<Task<HttpResponseMessage>> executionFunc,
                                                                  string? serviceName = null)
        {
           var response = await EnsureRequestResultIsSuccessful(executionFunc, serviceName);

           var responseAsString = await response.Content.ReadAsStringAsync();

            if (typeof(TResult).IsPrimitive)
            {
                return (TResult)Convert.ChangeType(responseAsString, typeof(TResult))
                    ?? throw new NullReferenceException(nameof(responseAsString));
            }
            else if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)responseAsString
                    ?? throw new NullReferenceException(nameof(responseAsString));
            }
            else
            {
                var deserialized = JsonConvert.DeserializeObject<TResult>(responseAsString);
                return deserialized ?? throw new NullReferenceException(nameof(deserialized));
            }

        }

        ///<inheritdoc/>
        public Task ProccessRequestResult(Func<Task<HttpResponseMessage>> executionFunc,
                                          string? serviceName = null) =>
            EnsureRequestResultIsSuccessful(executionFunc, serviceName);

        private static async Task<HttpResponseMessage> EnsureRequestResultIsSuccessful(Func<Task<HttpResponseMessage>> executionFunc,
                                                                                        string? serviceName)
        {
            var response = await executionFunc();

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                throw new DownstreamServiceRequestFailedException(
                    serviceName: serviceName ?? string.Empty,
                    message: $"Response from {serviceName ?? "downstream_unknown"} service - {response.StatusCode} - {response?.Content}",
                    body: responseContent,
                    downstreamResponseCode: response?.StatusCode);
            }

            return response;
        }
    }
}
