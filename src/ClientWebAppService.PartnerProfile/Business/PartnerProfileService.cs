using ClientWebAppService.PartnerProfile.Business.Models;
using ClientWebAppService.PartnerProfile.Business.Utils;
using ClientWebAppService.PartnerProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Helpers;
using CXI.Common.Models.Pagination;
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

                var partnerWithSuchAddressOrName = await _partnerRepository.FindOne(x => x.PartnerName == creationModel.Name && x.Address == creationModel.Address);

                if (partnerWithSuchAddressOrName is not null)
                {
                    throw new ValidationException(nameof(creationModel.Address), $"Such address {creationModel.Address} and name {creationModel.Name} already exists.");
                }

                var newPartnerProfile = new Partner
                {
                    PartnerName = creationModel.Name,
                    Address = creationModel.Address,
                    PartnerId = PartnerProfileUtils.GetPartnerIdByName(creationModel.Name),
                    PartnerType = PartnerProfileUtils.DefaultPartnerType,
                    ServiceAgreementAccepted = false,
                    Subscription = new Subscription
                    {
                        SubscriptionId = string.Empty,
                        State = null,
                        LastBilledDate = null
                    },
                    ServiceAgreementVersion = string.Empty,
                    ServiceAgreementAcceptedDate = null,
                    SyntheticGenerateFlag = true,
                    UiEnableFlag = true,
                    DemogPredictFlag = true,
                    TutorialEnableFlag = true,
                    CreatedOn = DateTime.UtcNow,
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

        public async Task<IEnumerable<string>> GetPartnerProfileIds()
        {
            var result = (await _partnerRepository.FilterBy()).Select(x => x.PartnerId);
            return result;
        }

        /// <inheritdoc cref="GetPartnerProfilesPaginatedAsync(int, int)"/>
        public async Task<PartnerProfilePaginatedDto> GetPartnerProfilesPaginatedAsync(int pageIndex, int pageSize)
        {
            VerifyHelper.GreaterThanZero(pageIndex, nameof(pageIndex));
            VerifyHelper.GreaterThanZero(pageSize, nameof(pageSize));

            var partnerProfiles = await _partnerRepository.GetPaginatedList(new PaginationRequest() { PageIndex = pageIndex, PageSize = pageSize });

            VerifyHelper.NotNull(partnerProfiles, nameof(partnerProfiles));

            return MapDto(partnerProfiles);
        }

        ///<inheritdoc/>
        public async Task<PaginatedResponse<PartnerProfileDto>> GetPartnerProfilesPaginatedAsync(PaginationRequest request)
        {
            VerifyHelper.GreaterThanZero(request.PageIndex, nameof(request.PageIndex));
            VerifyHelper.GreaterThanZero(request.PageSize, nameof(request.PageSize));

            var result = await _partnerRepository.GetPaginatedList(request);

            VerifyHelper.NotNull(result, nameof(result));

            return MapToPaginatedResponse(result);
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
                    UserProfiles = updateModel.UserProfileEmails,
                    ServiceAgreementAccepted = updateModel.ServiceAgreementAccepted,
                    IsActive = updateModel.IsActive,
                    Subscription = updateModel.Subscription,
                    ServiceAgreementVersion = updateModel.ServiceAgreementVersion,
                    ServiceAgreementAcceptedDate = updateModel.ServiceAgreementAcceptedDate
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

        /// <inheritdoc cref="UpdatePartnerSubscriptionAsync(string, SubscriptionUpdateModel)"/>
        public async Task UpdatePartnerSubscriptionAsync(string partnerId, SubscriptionUpdateModel model)
        {
            VerifyHelper.NotNull(model, nameof(model));

            var subscription = new Subscription 
            {
                SubscriptionId = model.SubscriptionId.ToString(),
                State = model.State,
                LastBilledDate = model.LastBilledDate
            };

            await _partnerRepository.UpdateSubscriptionAsync(partnerId, subscription);
        }

        ///<inheritdoc/>
        public async Task CompletePartnerOnBoardingAsync(string partnerId)
        {
            _logger.LogInformation($"Finalizing on-boarding for the partner with Id {partnerId}.");

            var partner = await _partnerRepository.FindOne(x => x.PartnerId == partnerId);

            if (partner is null)
            {
                throw new NotFoundException($"PartnerProfile with id: {partnerId} not found.");
            }

            await _partnerRepository.CompletePartnerOnBoarding(partnerId);

            _logger.LogInformation($"Partner with Id {partnerId} was successfully on-boarded.");
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
            new(partner.PartnerId, partner.PartnerName, partner.Address, partner.PartnerType,
                partner.AmountOfLocations, partner.ServiceAgreementAccepted, partner.UserProfiles,
                partner.IsActive, partner.Subscription, partner.CreatedOn, partner.IsOnBoarded,
                partner.ServiceAgreementVersion, partner.ServiceAgreementAcceptedDate,partner.SyntheticGenerateFlag,partner.UiEnableFlag,partner.DemogPredictFlag,partner.TutorialEnableFlag, partner.OverviewDashboardFlag);

        /// <inheritdoc cref = "IPartnerProfileService.SearchPartnerIdsByActiveStateAsync(bool?)" />
        public async Task<List<string>> SearchPartnerIdsByActiveStateAsync(bool? active)
        {
            _logger.LogInformation($"Get partner information with active : {active}");

            var activePartner = await _partnerRepository.FilterBy(active != null ? partner => partner.IsActive == active : null);
            var partnerIds = activePartner.Select(x=> x.PartnerId).ToList();

            _logger.LogInformation($"Successfully got partner information with active : {active}");

            return partnerIds;
        }

        private PartnerProfilePaginatedDto MapDto(PaginatedResponse<Partner> model)
        {
            return new PartnerProfilePaginatedDto
            {
                Items = model.Items.Select(Map),
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                TotalCount = model.TotalCount,
                TotalPages = model.TotalPages
            };
        }
        
        /// <inheritdoc cref="UpdatePartnerSubscriptionsAsync(List<SubscriptionPartnerIdDto>)"/>
        public async Task UpdatePartnerSubscriptionsAsync(IEnumerable<SubscriptionBulkUpdateDto> subscriptionBulkUpdateDtos)
        {
            await _partnerRepository.UpdateSubscriptionsAsync(subscriptionBulkUpdateDtos);
        }

        private PaginatedResponse<PartnerProfileDto> MapToPaginatedResponse(PaginatedResponse<Partner> model)
        {
            return new PaginatedResponse<PartnerProfileDto>
            {
                Items = model.Items.Select(Map).ToList(),
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                TotalCount = model.TotalCount,
                TotalPages = model.TotalPages
            };
        }

        /// <summary>
        /// SetPartnerStatusAsync
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task SetPartnerActivityStatusAsync(string partnerId, bool value)
        {
            var partner = await _partnerRepository.FindOne(x => x.PartnerId == partnerId);
            if (partner is null)
            {
                throw new NotFoundException($"PartnerProfile with id: {partnerId} not found.");
            }

            await _partnerRepository.SetActivityStatus(partnerId, value);

            _logger.LogInformation($"Partner {partnerId} actiity status was set to {value}.");
        }

        ///<inheritdoc/>
        public async Task<PartnerProfileDto?> FindPartnerProfileAsync(string partnerId)
        {
            _logger.LogInformation($"Get partner profile with id : {partnerId}.");

            if (string.IsNullOrWhiteSpace(partnerId))
            {
                throw new ValidationException(nameof(partnerId), $"{nameof(partnerId)} should not be null or empty.");
            }

            var result = await _partnerRepository.FindOne(x => x.PartnerId == partnerId);

            return result != null ? Map(result) : null;
        }

        ///<inheritdoc/>
        public async Task DeleteProfileByPartnerIdAsync(string partnerId)
        {
            _logger.LogInformation($"Deleting partner profile with id : {partnerId}.");
            VerifyHelper.NotEmptyOrWhiteSpace(partnerId, nameof(partnerId));           

            var partnerToDelete = await _partnerRepository.FindOne(x => x.PartnerId == partnerId);

            if (partnerToDelete == null)
            {
                throw new NotFoundException($"DeleteProfileByPartnerIdAsync. PartnerProfile with partnerId: {partnerId} not found.");
            }

            await _partnerRepository.DeleteOne(x => x.PartnerId == partnerId);
        }

        ///<inheritdoc/>
        public async Task UpdateProcessConfigurationAsync(string partnerId, ProcessConfigurationUpdateModel processConfiguration)
        {
            try
            {
                _logger.LogInformation($"Updating partner profile with id : {partnerId}.");
                Partner newPart = new Partner
                {
                    SyntheticGenerateFlag = processConfiguration.SyntheticGenerateFlag,
                    UiEnableFlag = processConfiguration.UiEnableFlag,
                    DemogPredictFlag = processConfiguration.DemogPredictFlag,
                    OverviewDashboardFlag = processConfiguration.OverviewDashboardFlag
                };

                await _partnerRepository.UpdateProcessConfigAsync(partnerId, newPart);
                _logger.LogInformation($"Successfully updated partner profile configuration with id : {partnerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateProfileAsync - Attempted to update partner profile with id : {partnerId}, Exception message - {ex.Message}");
                throw;
            }
        }

        ///<inheritdoc/>
        public async Task UpdatePartnerTutorialConfigurationAsync(string partnerId, bool tutorialEnableFlag)
        {
            try
            {
                _logger.LogInformation($"Updating partner tutorialEnableFlag with id : {partnerId}.");
                Partner newPart = new Partner
                {
                    TutorialEnableFlag = tutorialEnableFlag
                };

                await _partnerRepository.UpdateTutorialConfigAsync(partnerId, newPart);
                _logger.LogInformation($"Successfully updated partner profile configuration with id : {partnerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateProfileAsync - Attempted to update partner profile with id : {partnerId}, Exception message - {ex.Message}");
                throw;
            }
        } 
    }
}
