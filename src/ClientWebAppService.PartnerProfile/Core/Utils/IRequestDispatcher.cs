using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.Core.Utils
{
    /// <summary>
    /// Provide logic for dispatching HttpResponseMessage.
    /// </summary>
    public interface IRequestDispatcher
    {
        /// <summary>
        /// Check that returned by <paramref name="executionFunc"/> <see cref="HttpResponseMessage"/> have success status code and map response to <typeparamref name="TResult"/>.
        /// Otherwise throw <see cref="Exceptions.DownstreamServiceRequestFailedException"/>
        /// </summary>
        /// <typeparam name="TResult">Expected result type./</typeparam>
        Task<TResult> DispatchRequestResult<TResult>(Func<Task<HttpResponseMessage>> executionFunc,
                                                     string? serviceName = null);

        /// <summary>
        /// Check that returned by <paramref name="executionFunc"/> <see cref="HttpResponseMessage"/> have success status code.
        /// Otherwise throw <see cref="Exceptions.DownstreamServiceRequestFailedException"/>
        /// </summary>
        Task ProccessRequestResult(Func<Task<HttpResponseMessage>> executionFunc,
                                   string? serviceName = null);
    }
}
