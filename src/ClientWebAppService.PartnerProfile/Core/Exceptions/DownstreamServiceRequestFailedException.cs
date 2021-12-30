using CXI.Common.ExceptionHandling.Primitives;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace ClientWebAppService.PartnerProfile.Core.Exceptions
{
    /// <summary>
    /// Exception that represent HttpStatusCode.FailedDependency 424 Status code. Thowed when downstream service request failed.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DownstreamServiceRequestFailedException : BusinessException
    {
        public DownstreamServiceRequestFailedException(string serviceName,
                                                       string message,
                                                       string? body = null,
                                                       HttpStatusCode? downstreamResponseCode = null)
            : base(HttpStatusCode.FailedDependency, message)
        {
            DownstreamServiceName = serviceName;
            DownstreamServiceResponseCode = downstreamResponseCode;
            DownstreamServiceResponseBody = body;
        }

        public DownstreamServiceRequestFailedException(string serviceName,
                                                       string message)
            : base(HttpStatusCode.FailedDependency, message)
        {
            DownstreamServiceName = serviceName;
            DownstreamServiceResponseCode = HttpStatusCode.InternalServerError;
        }

        public DownstreamServiceRequestFailedException(string serviceName,
                                                       string message,
                                                       string? body)
            : base(HttpStatusCode.FailedDependency, message)
        {
            DownstreamServiceName = serviceName;
            DownstreamServiceResponseCode = HttpStatusCode.InternalServerError;
            DownstreamServiceResponseBody = body;
        }

        public DownstreamServiceRequestFailedException(string serviceName)
            : base(HttpStatusCode.FailedDependency, string.Empty)
        {
            DownstreamServiceName = serviceName;
            DownstreamServiceResponseCode = HttpStatusCode.InternalServerError;
        }

        public string DownstreamServiceName { get; set; }

        public HttpStatusCode? DownstreamServiceResponseCode { get; set; }

        public string? DownstreamServiceResponseBody { get; set; }

        protected override IDictionary<string, object> SetupExtensions() =>
            new Dictionary<string, object>()
            {
                {nameof(DownstreamServiceName) , DownstreamServiceName},
                {nameof(DownstreamServiceResponseBody) , DownstreamServiceResponseBody?? string.Empty},
                {nameof(DownstreamServiceResponseCode) , (int)(DownstreamServiceResponseCode ?? HttpStatusCode.InternalServerError)},
            };
    }
}
