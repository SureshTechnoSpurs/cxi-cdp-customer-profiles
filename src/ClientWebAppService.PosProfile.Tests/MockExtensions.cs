using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClientWebAppService.PosProfile.Tests
{
    public static class MockExtensions
    {
        public static Mock<ILogger<T>> VerifyLogWasCalled<T>(this Mock<ILogger<T>> logger, string expectedMessage, LogLevel logLevel)
        {
            Func<object, Type, bool> state = (v, t) => v.ToString().Contains(expectedMessage);
    
            logger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == logLevel),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => state(v, t)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            return logger;
        }
    }
}