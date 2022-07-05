using System;
using System.Collections.Generic;
using CXI.Common.Helpers;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.DependencyInjection;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    public class PosCredentialsServiceResolver : IPosCredentialsServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public PosCredentialsServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPosCredentialsService<T> Resolve<T>(T _)
        {
            return _serviceProvider.GetService<IPosCredentialsService<T>>() ??
                   throw new KeyNotFoundException($"service not found for type {typeof(T).Name}");
        }

        public IPosCredentialsOffboardingService ResolveOffboardingService(string posType)
        {
            switch (posType)
            {
                case "omnivore":
                    var omnivoreService = _serviceProvider
                        .GetService<IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>>();
                    VerifyHelper.NotNull(omnivoreService, nameof(IPosCredentialsService<PosCredentialsConfigurationOmnivoreCreationDto>));
                    return (IPosCredentialsOffboardingService)omnivoreService;
                
                case "square":
                    var squareService = _serviceProvider
                        .GetService<IPosCredentialsService<PosCredentialsConfigurationSquareCreationDto>>();
                    VerifyHelper.NotNull(squareService, nameof(IPosCredentialsService<PosCredentialsConfigurationSquareCreationDto>));
                    return (IPosCredentialsOffboardingService)squareService;

                case "olo":
                    var oloService = _serviceProvider
                        .GetService<IPosCredentialsService<PosCredentialsConfigurationOloCreationDto>>();
                    VerifyHelper.NotNull(oloService, nameof(IPosCredentialsService<PosCredentialsConfigurationOloCreationDto>));
                    return (IPosCredentialsOffboardingService)oloService;


                case "parbrink":
                    var parBrinkService = _serviceProvider
                        .GetService<IPosCredentialsService<PosCredentialsConfigurationParBrinkCreationDto>>();
                    VerifyHelper.NotNull(parBrinkService, nameof(IPosCredentialsService<PosCredentialsConfigurationParBrinkCreationDto>));
                    return (IPosCredentialsOffboardingService)parBrinkService;

                case "toast":
                    var toastService = _serviceProvider
                        .GetService<IPosCredentialsService<PosCredentialsConfigurationToastCreationDto>>();
                    VerifyHelper.NotNull(toastService, nameof(IPosCredentialsService<PosCredentialsConfigurationToastCreationDto>));
                    return (IPosCredentialsOffboardingService)toastService;

                default:
                    throw new KeyNotFoundException($"service not found for type {posType}");

            }
        }
    }
}