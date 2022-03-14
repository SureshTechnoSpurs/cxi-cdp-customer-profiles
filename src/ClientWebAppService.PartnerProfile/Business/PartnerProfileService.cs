using ClientWebAppService.PartnerProfile.Business.Models;
using ClientWebAppService.PartnerProfile.Business.Utils;
using ClientWebAppService.PartnerProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Contracts.PartnerProfile.Models;
using CXI.Contracts.PosProfile;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ValidationException = CXI.Common.ExceptionHandling.Primitives.ValidationException;

namespace ClientWebAppService.PartnerProfile.Business
{
    ///<inheritdoc/>
    public class PartnerProfileService : IPartnerProfileService
    {
        private readonly ILogger<PartnerProfileService> _logger;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IPosProfileServiceClient _posProfileServiceClient;

        public PartnerProfileService(IPartnerRepository partnerRepository,
                                     ILogger<PartnerProfileService> logger,
                                     IPosProfileServiceClient posProfileServiceClient)
        {
            _partnerRepository = partnerRepository;
            _logger = logger;
            _posProfileServiceClient = posProfileServiceClient;
        }

        ///<inheritdoc/>
        public async Task<PartnerProfileDto> CreateProfileAsync(PartnerProfileCreationModel creationModel)
        {
            try
            {
                _logger.LogInformation($"Creating partner profile with name : {creationModel.Name}.");

                var partnerWithSuchAddressOrName = await _partnerRepository.FindOne(x => x.Address == creationModel.Address || x.PartnerName == creationModel.Name);

                if (partnerWithSuchAddressOrName is not null)
                {
                    throw new ValidationException(nameof(creationModel.Address), $"Such address {creationModel.Address} or name {creationModel.Name} already presented.");
                }

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
        public async Task<IEnumerable<PartnerProfileDto>> GetPartnerProfilesAsync()
        {
            _logger.LogInformation("Getting all partner profiles.");

            var result = await _partnerRepository.FilterBy();

            var partnerProfiles = result.ToList();

            if (!partnerProfiles.Any())
            {
                throw new NotFoundException("Partner profiles don't exist.");
            }

            return partnerProfiles.Select(Map);
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

        ///<inheritdoc/>
        public async Task<IEnumerable<PosTypePartnerDto>> GetActivePartnersByPosTypeAsync(string posType)
        {
            if (string.IsNullOrWhiteSpace(posType))
            {
                throw new ValidationException(nameof(posType), "POS type can not be null or empty.");
            }

            var response = await _posProfileServiceClient.GetPosProfileIdsByPosTypeAsync(posType);

            // filter out active profiles
            var posTypePartners = response.ToList();

            return posTypePartners.Select(partnerId =>
            {
                return new PosTypePartnerDto(partnerId, PartnerProfileUtils.DefaultPartnerCountry, partnerId);
            });
        }

        private PartnerProfileDto Map(Partner partner) =>
            new(partner.PartnerId, partner.PartnerName, partner.Address, partner.PartnerType, partner.AmountOfLocations, partner.UserProfiles);
    }
}
