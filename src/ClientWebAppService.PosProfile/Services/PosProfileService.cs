using Azure;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Helpers;
using CXI.Common.Security.Secrets;
using CXI.Contracts.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Services.Credentials;
using PosCredentialsConfigurationDto = CXI.Contracts.PosProfile.Models.PosCredentialsConfigurationDto;

namespace ClientWebAppService.PosProfile.Services
{
    /// <inheritdoc cref="IPosProfileService"/>
    public class PosProfileService : IPosProfileService
    {
        private const string SecretNotFoundErrorCode = "SecretNotFound";

        private readonly IPosProfileRepository _posProfileRepository;
        private readonly ILogger<PosProfileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISecretClient _secretClient;
        private readonly IPosCredentialsServiceResolver _credentialsServiceResolver;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="posProfileRepository">DAO object for accessing PosProfile MongoDB collection</param>
        /// <param name="credentialsServiceResolver"></param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration"></param>
        /// <param name="secretClient"></param>
        public PosProfileService(
            IPosProfileRepository posProfileRepository,
            ILogger<PosProfileService> logger, 
            IConfiguration configuration,
            ISecretClient secretClient,
            IPosCredentialsServiceResolver credentialsServiceResolver)
        {
            _posProfileRepository = posProfileRepository;
            _logger = logger;
            _configuration = configuration;
            _secretClient = secretClient;
            _credentialsServiceResolver = credentialsServiceResolver;
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> CreatePosProfileAndSecretsAsync<T>(PosProfileCreationModel<T> posProfileCreationDto) where T: IPosCredentialsConfigurationBaseDto
        {
            VerifyHelper.NotNull(posProfileCreationDto, nameof(posProfileCreationDto));

            _logger.LogInformation($"Creating new Pos Profile for partnerId = {posProfileCreationDto.PartnerId}");

            int.TryParse(_configuration.GetDefaultHistoricalIngestPeriod(), out var defaultHistoricalIngestPeriod);

            var posProfile = new Models.PosProfile
            {
                PartnerId = posProfileCreationDto.PartnerId,
                PosConfiguration = new List<PosCredentialsConfiguration>(),
                HistoricalIngestDaysPeriod = defaultHistoricalIngestPeriod
            };
                
            var posCredentialsService = _credentialsServiceResolver.Resolve(posProfileCreationDto.PosConfigurations);

            ((List<PosCredentialsConfiguration>)posProfile.PosConfiguration).Add(
                await posCredentialsService.Process(posProfileCreationDto.PartnerId, posProfileCreationDto.PosConfigurations));

            await ValidateAndUpdatePosConfiguration(posProfile);

            _logger.LogInformation($"Successfully created pos profiler for partnerId = {posProfileCreationDto.PartnerId}");

            return new PosProfileDto(posProfile.PartnerId,
                                     posProfile.PosConfiguration.Select(x =>
                                        new PosCredentialsConfigurationDto(x.PosType, x.KeyVaultReference, x.MerchantId)
                                     ));
        }

        private async Task ValidateAndUpdatePosConfiguration(Models.PosProfile posProfile)
        {
            VerifyHelper.NotEmpty(posProfile.PartnerId, nameof(posProfile.PartnerId));

            var partnerId = posProfile.PartnerId;
            var existingPosProfile = await FindPosProfileByPartnerIdAsync(partnerId);
            if (existingPosProfile != null)
            {
                VerifyHelper.NotNull(posProfile.PosConfiguration, nameof(posProfile.PosConfiguration));
                var newPosConfiguration = posProfile.PosConfiguration.First();
                var posType = newPosConfiguration.PosType;
                var existingPosTypes = existingPosProfile.PosCredentialsConfigurations.Select(x => x.PosType).ToList();

                if (existingPosTypes.Contains(posType))
                {
                    throw new Exception($"Pos Configuration already exists for pos type : {posType}");
                }
                else
                {
                    await UpdatePosConfiguration(posProfile, existingPosProfile, newPosConfiguration);
                }

            }
            else
            {
                await _posProfileRepository.InsertOne(posProfile);
            }
        }

        private async Task UpdatePosConfiguration(Models.PosProfile posProfile, PosProfileDto existingPosProfile, PosCredentialsConfiguration newConfiguration)
        {
            var newPosConfigurations = new List<PosCredentialsConfiguration>();
            var partnerId = posProfile.PartnerId;

            foreach (var configuration in existingPosProfile.PosCredentialsConfigurations)
            {
                newPosConfigurations.Add(new PosCredentialsConfiguration
                {
                    PosType = configuration.PosType,
                    KeyVaultReference = configuration.KeyVaultReference,
                    MerchantId = configuration.MerchantId

                });
            }

            newPosConfigurations.Add(newConfiguration);
            posProfile.PosConfiguration = newPosConfigurations;
            await _posProfileRepository.UpdateAsync(partnerId, posProfile);
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

        /// <inheritdoc cref="GetAccesTokenForPartner(string)" />
        public async Task<string> GetAccesTokenForPartner(string partnerId)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));

            var posProfile = (await _posProfileRepository.FilterBy(x => x.PartnerId == partnerId)).FirstOrDefault();
            if (posProfile == null)
            {
                return null;
            }

            var posConfiguration = posProfile.PosConfiguration?.FirstOrDefault();
            if (posConfiguration == null)
            {
                return null;
            }

            var posConfigurationSecretName = posConfiguration.KeyVaultReference;

            VerifyHelper.NotEmpty(posConfigurationSecretName, nameof(posConfigurationSecretName));
            var secret = _secretClient.GetSecret(posConfigurationSecretName);

            VerifyHelper.NotNull(secret, nameof(secret));
            var secretPayload = JsonConvert.DeserializeObject<PosProfileSecretConfiguration>(secret.Value);

            return secretPayload?.AccessToken?.Value;
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> FindPosProfileByPartnerIdAsync(string partnerId)
        {
            _logger.LogInformation($"Find pos profile for partnerId = {partnerId}");

            var posProfile = await _posProfileRepository.FindOne(pp => pp.PartnerId != null && pp.PartnerId.Equals(partnerId));

            return posProfile != null ? new PosProfileDto(posProfile.PartnerId,
                                                          posProfile.PosConfiguration.Select(x =>
                                                             new PosCredentialsConfigurationDto(x.PosType, x.KeyVaultReference, x.MerchantId)))
                : throw new NotFoundException($"PosProfile with partnerId:{partnerId} not found.")
                                      ;
        }

        /// <inheritdoc/>
        public async Task<PosProfileDto> GetPosProfileByPartnerId(string partnerId)
        {
            _logger.LogInformation($"Getting POS profile for partnerId: {partnerId}.");

            var result = await _posProfileRepository.FilterBy(x => x.PartnerId == partnerId);

            var posProfile = result.FirstOrDefault();

            if (posProfile == null)
            {
                throw new NotFoundException($"Pos profile for partnerId: {partnerId} not found.");
            }

            return new PosProfileDto(posProfile.PartnerId, posProfile.PosConfiguration.Select(x =>
                    new PosCredentialsConfigurationDto(x.PosType, x.KeyVaultReference, x.MerchantId)));
        }

        /// <inheritdoc cref="GetByMerchantId(string)" />
        public async Task<PosProfileDto> GetByMerchantId(string merchantId)
        {
            VerifyHelper.NotEmpty(merchantId, nameof(merchantId));

            var posProfile = await _posProfileRepository.FindOne(x => x.PosConfiguration.Any(c => c.MerchantId == merchantId));
            if (posProfile == null)
            {
                throw new NotFoundException($"PosProfile with merchantId:{merchantId} not found.");
            }

            return new PosProfileDto(
                posProfile.PartnerId,
                posProfile.PosConfiguration.Select(x => new PosCredentialsConfigurationDto(x.PosType, x.KeyVaultReference, x.MerchantId)));
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
                                               x.PosConfiguration?.Select(pc => new PosCredentialsConfigurationDto(pc.PosType, pc.KeyVaultReference, pc.MerchantId)),
                                               x.IsHistoricalDataIngested,
                                               x.HistoricalIngestDaysPeriod);
            });
        }

        /// <inheritdoc cref="GetPosProfilesByPartnerIdsAsync(IEnumerable{string})"/>
        public async Task<IEnumerable<PosProfileDto>> GetPosProfilesByPartnerIdsAsync(IEnumerable<string> partnerIds)
        {
            VerifyHelper.CollectionNotEmpty(partnerIds, nameof(partnerIds));

            var result = await _posProfileRepository.FilterBy(x => partnerIds.Contains(x.PartnerId));

            return result.Select(x => new PosProfileDto(
                x.PartnerId, 
                x.PosConfiguration.Select(pc => new PosCredentialsConfigurationDto(
                    pc.PosType, 
                    pc.KeyVaultReference, 
                    pc.MerchantId))));
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