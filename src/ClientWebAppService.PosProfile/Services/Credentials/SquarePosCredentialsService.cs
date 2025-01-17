﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.Logging;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    ///     Square realization for pos credentials service
    /// </summary>
    public class SquarePosCredentialsService : IPosCredentialsService<PosCredentialsConfigurationSquareCreationDto>, IPosCredentialsOffboardingService
    {
        private const string AuthenticationScheme = "Bearer";
        private const string SquarePosType = "square";
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private readonly ISecretSetter _secretSetter;
        private readonly ISecretClient _secretClient;
        private readonly ILogger<SquarePosCredentialsService> _logger;

        public SquarePosCredentialsService(ISecretSetter secretSetter, ISecretClient secretClient, ILogger<SquarePosCredentialsService> logger)
        {
            _secretSetter = secretSetter;
            _secretClient = secretClient;
            _logger = logger;
        }

        public async Task OffboardingProcess(string partnerId)
        {
            try
            {
                var posConfigurationSecretName = GetPosConfigurationSecretName(partnerId, SquarePosType);
                await _secretClient.DeleteSecretAsync(posConfigurationSecretName);

                var posConfigurationDataIngestionSecretName = GetPosConfigurationDataIngestionSecretName(partnerId, SquarePosType);
                await _secretClient.DeleteSecretAsync(posConfigurationDataIngestionSecretName);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == SecretNotFoundErrorCode)
            {
                _logger.LogError(ex, $"Secret was not found in key vault for ${partnerId}");
                throw;
            }
        }

        /// <summary>
        ///     Convert square create dto to PosCredentialsConfiguration and save bearer tokens to key-vault
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posConfigurationDto"></param>
        /// <returns></returns>
        public async Task<PosCredentialsConfiguration> OnboardingProcess(string partnerId, PosCredentialsConfigurationSquareCreationDto posConfigurationDto)
        {
            var savedSecretNames = new List<string>();
            try
            {
                string posConfigurationJsonSecret = SecretExtensions.ComposePosConfigurationSecretPayload(posConfigurationDto);
                var posConfigurationSecretName = SecretExtensions.GetPosConfigurationSecretName(partnerId, posConfigurationDto.PosType);

                _secretSetter.Set(posConfigurationSecretName, posConfigurationJsonSecret, null);
                savedSecretNames.Add(posConfigurationSecretName);

                var tokenInfo = SecretExtensions.ComposeSecretPayloadForDataCollectService(posConfigurationDto.PosType, partnerId, posConfigurationDto.AccessToken, AuthenticationScheme);
                _secretSetter.Set(tokenInfo.keyVaultSecretName, tokenInfo.keyVaultSecretValue, null);
                savedSecretNames.Add(tokenInfo.keyVaultSecretName);

                _logger.LogInformation($"Processing posProfile with partnerId={partnerId}, merchantId={posConfigurationDto.MerchantId}");

                return new PosCredentialsConfiguration
                {
                    PosType = posConfigurationDto.PosType,
                    KeyVaultReference = posConfigurationSecretName,
                    MerchantId = posConfigurationDto.MerchantId
                };
            }
            catch (Exception exception)
            {
                _logger.LogError($"CreatePosProfileAsync - Attempted to create profile for ${partnerId}, Exception message - {exception.Message}");

                if (savedSecretNames.Any())
                {
                    _logger.LogInformation($"Revert secrets for partnerId = {partnerId}");
                    foreach (var secretName in savedSecretNames)
                    {
                        await _secretClient.DeleteSecretAsync(secretName);
                    }
                }

                throw;
            }
        }

        private string GetPosConfigurationSecretName(string partnerId, string posType)
        {
            return $"{partnerId}-{posType}";
        }

        private string GetPosConfigurationDataIngestionSecretName(string partnerId, string posType)
        {
            return $"di-{posType}-{partnerId}-tokeninfo";
        }
    }
}