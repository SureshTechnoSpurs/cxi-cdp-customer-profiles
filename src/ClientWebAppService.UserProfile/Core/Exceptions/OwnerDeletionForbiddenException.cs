using CXI.Common.ExceptionHandling.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace ClientWebAppService.UserProfile.Core.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class OwnerDeletionForbiddenException : BaseException
    {
        public OwnerDeletionForbiddenException() : base(HttpStatusCode.Conflict)
        {
        }

        public OwnerDeletionForbiddenException(string message)
            : base(HttpStatusCode.Conflict, message)
        {
        }

        public OwnerDeletionForbiddenException(string message, Exception inner)
            : base(HttpStatusCode.Conflict, message, inner)
        {
        }
    }
}
