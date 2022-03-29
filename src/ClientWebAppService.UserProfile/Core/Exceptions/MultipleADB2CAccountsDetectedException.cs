using CXI.Common.ExceptionHandling.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace ClientWebAppService.UserProfile.Core.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class MultipleADB2CAccountsDetectedException : BaseException
    {
        public MultipleADB2CAccountsDetectedException() : base(HttpStatusCode.Conflict)
        {
        }

        public MultipleADB2CAccountsDetectedException(string message)
            : base(HttpStatusCode.Conflict, message)
        {
        }

        public MultipleADB2CAccountsDetectedException(string message, Exception inner)
            : base(HttpStatusCode.Conflict, message, inner)
        {
        }
    }
}
