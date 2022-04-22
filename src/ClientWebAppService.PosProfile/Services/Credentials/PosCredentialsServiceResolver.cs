using System;
using System.Collections.Generic;
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
    }
}