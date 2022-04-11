using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Services.Credentials;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ClientWebAppService.PosProfile.Tests
{
    public class PosCredentialsServiceResolverTests
    {
        private PosCredentialsServiceResolver _resolver;

        [Fact]
        public void Resolve_ServiceExistInCollection_ServiceCorrectTypeReturned()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>, OmnivorePosCredentialsService>();
            var sp = serviceCollection.BuildServiceProvider();
            _resolver = new PosCredentialsServiceResolver(sp);

            var service = _resolver.Resolve(new PosCredentialsConfigurationOmnivoreCreationDto("omnivore"));
            Assert.IsType<OmnivorePosCredentialsService>(service);
        }

        [Fact]
        public void Resolve_ServiceNotExistInCollection_ExceptionThrown()
        {
            var serviceCollection = new ServiceCollection();
            var sp = serviceCollection.BuildServiceProvider();
            _resolver = new PosCredentialsServiceResolver(sp);

            var ex = Assert.Throws<KeyNotFoundException>(() => _resolver.Resolve(new PosCredentialsConfigurationOmnivoreCreationDto("omnivore")));
            Assert.Equal($"service not found for type {nameof(PosCredentialsConfigurationOmnivoreCreationDto)}", ex.Message);
        }
    }
}
