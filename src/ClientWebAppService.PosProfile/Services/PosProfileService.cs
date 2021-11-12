using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.DataAccess;
using ClientWebAppService.PosProfile.Models;
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

            return new PosProfileDto(posProfile?.PartnerId, posProfile?.PosConfiguration);
        }

        /// <inheritdoc cref="IPosProfileService"/>
        public async Task<PosProfileDto> GetPosProfileAsync(string partnerId)
        {
            var posProfile = await _posProfileRepository.FindOne(pp => pp.PartnerId != null && pp.PartnerId.Equals(partnerId));

            return new PosProfileDto(posProfile.PartnerId, posProfile.PosConfiguration);
        }

        private string ComposePosConfigurationSecretPayload(PosCredentialsConfigurationDto posCredentialsConfiguration)
        {
            var posProfileSecretConfiguration = new PosProfileSecretConfiguration(new AccessToken(Value: posCredentialsConfiguration.AccessToken, posCredentialsConfiguration.ExpirationDate), 
                                                     new RefreshToken(Value: posCredentialsConfiguration.RefreshToken, null));

            return JsonConvert.SerializeObject(posProfileSecretConfiguration);
        }
            
    }
}