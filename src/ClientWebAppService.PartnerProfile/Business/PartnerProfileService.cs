using ClientWebAppService.PartnerProfile.Business.Models;
using ClientWebAppService.PartnerProfile.Business.Utils;
using ClientWebAppService.PartnerProfile.DataAccess;
using ClientWebAppService.PartnerProfile.Models;
using CXI.Common.ExceptionHandling.Primitives;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ValidationException = CXI.Common.ExceptionHandling.Primitives.ValidationException;

namespace ClientWebAppService.PartnerProfile.Business
{
    ///<inheritdoc/>
    public class PartnerProfileService : IPartnerProfileService
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly ILogger<PartnerProfileService> _logger;

        public PartnerProfileService(IPartnerRepository partnerRepository,
                                     ILogger<PartnerProfileService> logger)
        {
            _partnerRepository = partnerRepository;
            _logger = logger;
        }

        ///<inheritdoc/>
        public async Task<PartnerProfileDto> CreateProfileAsync(PartnerProfileCreationModel creationModel)
        {
            try
            {
                _logger.LogInformation($"Creating partner profile with name : {creationModel.Name}.");
                var newPartnerProfile = new Partner
                {
                    PartnerName = creationModel.Name,
                    Address = creationModel.Address,
                    PartnerId = PartnerProfileUtils.GetPartnerIdByName(creationModel.Name),
                    PartnerType = PartnerProfileUtils.DefaultPartnerType
                };

                await _partnerRepository.InsertOne(newPartnerProfile);
                _logger.LogInformation($"Successfully created partner profile for partnerName : {creationModel.Name}");

                return Map(newPartnerProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateProfileAsync - Attempted to create partner profile with name: ${creationModel.Name}, Exception message - {ex.Message}");
                throw;
            }

        }

        ///<inheritdoc/>
        public async Task<PartnerProfileDto> GetByIdAsync(string partnerId)
        {
            _logger.LogInformation($"Get partner profile with id : {partnerId}.");

            if (string.IsNullOrWhiteSpace(partnerId))
            {
                throw new ValidationException(nameof(partnerId), $"{nameof(partnerId)} should not be null or empty.");
            }

            var result = await _partnerRepository.FindOne(x => x.PartnerId == partnerId);

            return result == null ? throw new NotFoundException($"PartnerProfile with id: {partnerId} not found.") : Map(result);
        }

        ///<inheritdoc/>
        public async Task UpdateProfileAsync(string partnerId, PartnerProfileUpdateModel updateModel)
        {
            try
            {
                _logger.LogInformation($"Updating partner profile with id : {partnerId}.");
                Partner newPart = new Partner
                {
                    PartnerName = updateModel.PartnerName,
                    PartnerType = updateModel.PartnerType,
                    AmountOfLocations = updateModel.AmountOfLocations,
                    Address = updateModel.Address,
                    UserProfiles = updateModel.UserProfileEmails
                };

                await _partnerRepository.UpdateAsync(partnerId, newPart);
                _logger.LogInformation($"Successfully updated partner profile with id : {partnerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateProfileAsync - Attempted to update partner profile with id : {partnerId}, Exception message - {ex.Message}");
                throw;
            }
        }

        private PartnerProfileDto Map(Partner partner) =>
            new PartnerProfileDto(partner.PartnerId, partner.PartnerName, partner.Address, partner.PartnerType, partner.AmountOfLocations, partner.UserProfiles);
    }
}
