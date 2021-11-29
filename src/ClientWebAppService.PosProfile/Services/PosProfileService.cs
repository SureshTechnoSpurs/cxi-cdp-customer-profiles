using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Security.Secrets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile.Services
{
    /// <inheritdoc cref="IPosProfileService"/>
    public class PosProfileService : IPosProfileService
    {
        private readonly IPosProfileRepository _posProfileRepository;
        private readonly ISecretSetter _secretSetter;
        private readonly ILogger<PosProfileService> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="posProfileRepository">DAO object for accessing PosProfile MongoDB collection</param>
        /// <param name="secretSetter">Encapsulates create operation for Key Vault secrets</param>
        /// <param name="logger">Logger</param>
        public PosProfileService(IPosProfileRepository posProfileRepository, ISecretSetter secretSetter, ILogger<PosProfileService> logger)
        {
            _posProfileRepository = posProfileRepository;
            _secretSetter = secretSetter;
            _logger = logger;
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> CreatePosProfileAsync(PosProfileCreationDto posProfileCreationDto)
        {
            Models.PosProfile posProfile;
            var posConfigurationJsonSecret = string.Empty;

            try
            {
                _logger.LogInformation($"Creating new Pos Profile for partnerId = {posProfileCreationDto.PartnerId}");

                posProfile = new Models.PosProfile
                {
                    PartnerId = posProfileCreationDto.PartnerId,
                    PosConfiguration = new List<PosCredentialsConfiguration>()
                };

                foreach (var posConfigurationDto in posProfileCreationDto.PosConfigurations)
                {
                    posConfigurationJsonSecret = this.ComposePosConfigurationSecretPayload(posConfigurationDto);

                    var keyVaultReferenceTemplate = $"{posProfileCreationDto.PartnerId}-{posConfigurationDto.PosType}";

                    ((List<PosCredentialsConfiguration>)posProfile.PosConfiguration).Add(new PosCredentialsConfiguration
                    {
                        PosType = posConfigurationDto.PosType,
                        KeyVaultReference = keyVaultReferenceTemplate
                    });

                    _secretSetter.Set(keyVaultReferenceTemplate, posConfigurationJsonSecret, null);
                }

                await _posProfileRepository.InsertOne(posProfile);
            }
            catch (Exception exception)
            {
                _logger.LogError($"CreatePosProfileAsync - Attempted to create profile for ${posProfileCreationDto.PartnerId}, Exception message - {exception.Message}");
                throw;
            }

            _logger.LogInformation($"Successfully created pos profiler for partnerId = {posProfileCreationDto.PartnerId}");
            return new PosProfileDto(posProfile?.PartnerId, posProfile?.PosConfiguration);
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> FindPosProfileByPartnerIdAsync(string partnerId)
        {
            var posProfile = await _posProfileRepository.FindOne(pp => pp.PartnerId != null && pp.PartnerId.Equals(partnerId));

            return posProfile == null ? throw new NotFoundException($"PosProfile with partnerId:{partnerId} not found.")
                                      : new PosProfileDto(posProfile.PartnerId, posProfile.PosConfiguration);
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<IEnumerable<PosProfileSearchDto>> GetPosProfilesAsync(PosProfileSearchCriteria searchCriteria)
        {
            var result = await _posProfileRepository.FilterBy(searchCriteria.IsHistoricalDataIngested != null ? profile =>
                                                                                   profile.IsHistoricalDataIngested == searchCriteria.IsHistoricalDataIngested : null);

            if (result == null)
            {
                throw new NotFoundException($"Pos profiles not found");
            }

            return result.Select(x =>
            {
                return new PosProfileSearchDto(x.PartnerId, x.PosConfiguration?.Select(pc => pc.PosType),
                    x.IsHistoricalDataIngested, x.HistoricalIngestDaysPeriod);
            });
        }

        /// <inheritdoc cref="IPosProfileService.UpdatePosProfileAsync"/>
        public async Task UpdatePosProfileAsync(string partnerId, PosProfileUpdateModel updateModel)
        {
            var posProfile = new Models.PosProfile
            {
                PosConfiguration = updateModel.PosConfigurations,
                IsHistoricalDataIngested = updateModel.IsHistoricalDataIngested
            };
            
            await _posProfileRepository.UpdateAsync(partnerId, posProfile);
        }

        private string ComposePosConfigurationSecretPayload(PosCredentialsConfigurationDto posCredentialsConfiguration)
        {
            var posProfileSecretConfiguration = new PosProfileSecretConfiguration(new AccessToken(Value: posCredentialsConfiguration.AccessToken, posCredentialsConfiguration.ExpirationDate),
                                                     new RefreshToken(Value: posCredentialsConfiguration.RefreshToken, null));

            return JsonConvert.SerializeObject(posProfileSecretConfiguration);
        }
    }
}