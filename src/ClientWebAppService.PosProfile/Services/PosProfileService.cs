using Azure;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Helpers;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PosCredentialsConfigurationDto = CXI.Contracts.PosProfile.Models.PosCredentialsConfigurationDto;

namespace ClientWebAppService.PosProfile.Services
{
    /// <inheritdoc cref="IPosProfileService"/>
    public class PosProfileService : IPosProfileService
    {
        private const string AuthenticationScheme = "Bearer";
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private readonly IPosProfileRepository _posProfileRepository;
        private readonly ISecretSetter _secretSetter;
        private readonly ILogger<PosProfileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISecretClient _secretClient;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="posProfileRepository">DAO object for accessing PosProfile MongoDB collection</param>
        /// <param name="secretSetter">Encapsulates create operation for Key Vault secrets</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration"></param>
        /// <param name="secretClient"></param>
        public PosProfileService(
            IPosProfileRepository posProfileRepository, 
            ISecretSetter secretSetter, 
            ILogger<PosProfileService> logger, 
            IConfiguration configuration,
            ISecretClient secretClient)
        {
            _posProfileRepository = posProfileRepository;
            _secretSetter = secretSetter;
            _logger = logger;
            _configuration = configuration;
            _secretClient = secretClient;
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> CreatePosProfileAndSecretsAsync(PosProfileCreationModel posProfileCreationDto)
        {
            Models.PosProfile posProfile;
            var savedSecretNames = new List<string>();

            try
            {
                _logger.LogInformation($"Creating new Pos Profile for partnerId = {posProfileCreationDto.PartnerId}");

                int.TryParse(_configuration.GetDefaultHistoricalIngestPeriod(), out var defaultHistoricalIngestPeriod);

                posProfile = new Models.PosProfile
                {
                    PartnerId = posProfileCreationDto.PartnerId,
                    PosConfiguration = new List<PosCredentialsConfiguration>(),
                    HistoricalIngestDaysPeriod = defaultHistoricalIngestPeriod
                };

                foreach (var posConfigurationDto in posProfileCreationDto.PosConfigurations)
                {
                    string posConfigurationJsonSecret = this.ComposePosConfigurationSecretPayload(posConfigurationDto);
                    var posConfigurationSecretName = GetPosConfigurationSecretName(posProfileCreationDto.PartnerId, posConfigurationDto.PosType);

                    ((List<PosCredentialsConfiguration>)posProfile.PosConfiguration).Add(new PosCredentialsConfiguration
                    {
                        PosType = posConfigurationDto.PosType,
                        KeyVaultReference = posConfigurationSecretName
                    });

                    _secretSetter.Set(posConfigurationSecretName, posConfigurationJsonSecret, null);
                    savedSecretNames.Add(posConfigurationSecretName);

                    var tokenInfo = ComposeSecretPayloadForDataCollectService(posConfigurationDto.PosType, posProfileCreationDto.PartnerId, posConfigurationDto.AccessToken);    
                    _secretSetter.Set(tokenInfo.keyVaultSecretName, tokenInfo.keyVaultSecretValue, null);
                    savedSecretNames.Add(tokenInfo.keyVaultSecretName);
                }

                await _posProfileRepository.InsertOne(posProfile);
            }
            catch (Exception exception)
            {
                _logger.LogError($"CreatePosProfileAsync - Attempted to create profile for ${posProfileCreationDto.PartnerId}, Exception message - {exception.Message}");

                if (savedSecretNames.Any())
                {
                    _logger.LogInformation($"Revert secrets for partnerId = {posProfileCreationDto.PartnerId}");
                    foreach (var secretName in savedSecretNames)
                    {
                        await _secretClient.DeleteSecretAsync(secretName);
                    }
                }

                throw;
            }

            _logger.LogInformation($"Successfully created pos profiler for partnerId = {posProfileCreationDto.PartnerId}");

            return new PosProfileDto(posProfile.PartnerId,
                                     posProfile.PosConfiguration.Select(x =>
                                        new PosCredentialsConfigurationDto(x.PosType, x.KeyVaultReference)
                                     ));
        }

        /// <inheritdoc cref="DeletePosProfileAndSecretsAsync(string)"/>
        public async Task DeletePosProfileAndSecretsAsync(string partnerId)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));

            _logger.LogInformation($"Removing PosProfile and secrets for partnerId = {partnerId}");

            try
            {
                var posProfiles = await _posProfileRepository.FilterBy(x => x.PartnerId == partnerId);
                if (!posProfiles.Any())
                {
                    return;
                }

                foreach (var posProfile in posProfiles)
                {
                    if (posProfile.PosConfiguration != null)
                    {
                        foreach (PosCredentialsConfiguration posCredentialsConfiguration in posProfile.PosConfiguration)
                        {
                            var posConfigurationSecretName = GetPosConfigurationSecretName(partnerId, posCredentialsConfiguration.PosType);
                            await _secretClient.DeleteSecretAsync(posConfigurationSecretName);

                            var posConfigurationDataIngestionSecretName = GetPosConfigurationDataIngestionSecretName(partnerId, posCredentialsConfiguration.PosType);
                            await _secretClient.DeleteSecretAsync(posConfigurationDataIngestionSecretName);
                        }
                    }
                }

                await _posProfileRepository.DeleteMany(x => x.PartnerId == partnerId);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == SecretNotFoundErrorCode)
            {
                _logger.LogError(ex, $"Secret was not found in key vault for ${partnerId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeletePosProfileAndSecretsAsync - failed for ${partnerId}");
                throw;
            }
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> FindPosProfileByPartnerIdAsync(string partnerId)
        {
            _logger.LogInformation($"Find pos profile for partnerId = {partnerId}");

            var posProfile = await _posProfileRepository.FindOne(pp => pp.PartnerId != null && pp.PartnerId.Equals(partnerId));

            return posProfile != null ? new PosProfileDto(posProfile.PartnerId,
                                                          posProfile.PosConfiguration.Select(x =>
                                                             new PosCredentialsConfigurationDto(x.PosType, x.KeyVaultReference)))
                : throw new NotFoundException($"PosProfile with partnerId:{partnerId} not found.")
                                      ;
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<IEnumerable<PosProfileSearchDto>> GetPosProfilesAsync(PosProfileSearchCriteriaModel searchCriteria)
        {

            _logger.LogInformation($"get pos profile by search Criteria");

            var result = await _posProfileRepository.FilterBy(searchCriteria.IsHistoricalDataIngested != null ? profile =>
                                                                                   profile.IsHistoricalDataIngested == searchCriteria.IsHistoricalDataIngested : null);

            var posProfiles = result.ToList();

            if (result == null || !posProfiles.Any())
            {
                throw new NotFoundException($"Pos profiles not found");
            }

            return posProfiles.Select(x =>
            {
                return new PosProfileSearchDto(x.PartnerId,
                                               x.PosConfiguration?.Select(pc => pc.PosType),
                                               x.IsHistoricalDataIngested,
                                               x.HistoricalIngestDaysPeriod);
            });
        }

        /// <inheritdoc cref="IPosProfileService.UpdatePosProfileAsync"/>
        public async Task UpdatePosProfileAsync(string partnerId, PosProfileUpdateModel updateModel)
        {
            _logger.LogInformation($"Update pos profile for partnerId = {partnerId}");

            try
            {
                var posProfile = new Models.PosProfile
                {
                    PosConfiguration = updateModel.PosConfigurations
                        .Select(x => new PosCredentialsConfiguration { KeyVaultReference = x.KeyVaultReference, PosType = x.PosType }),
                    IsHistoricalDataIngested = updateModel.IsHistoricalDataIngested
                };

                await _posProfileRepository.UpdateAsync(partnerId, posProfile);

                _logger.LogInformation($"Successfully updated pos profiler for partnerId = {partnerId}");
            }
            catch (Exception exception)
            {
                _logger.LogError($"UpdatePosProfileAsync - Attempted to update profile for partnerId = {partnerId}, Exception message - {exception.Message}");
                throw;
            }
        }

        private string ComposePosConfigurationSecretPayload(Models.PosCredentialsConfigurationDto posCredentialsConfiguration)
        {
            var posProfileSecretConfiguration = new PosProfileSecretConfiguration(new AccessToken(Value: posCredentialsConfiguration.AccessToken, posCredentialsConfiguration.ExpirationDate),
                                                     new RefreshToken(Value: posCredentialsConfiguration.RefreshToken, null));

            return JsonConvert.SerializeObject(posProfileSecretConfiguration);
        }

        private (string keyVaultSecretName, string keyVaultSecretValue) ComposeSecretPayloadForDataCollectService(string posType, string partnerId, string accessToken)
        {
            var secretName = GetPosConfigurationDataIngestionSecretName(partnerId, posType);
            return (secretName, $"{AuthenticationScheme} {accessToken}");
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