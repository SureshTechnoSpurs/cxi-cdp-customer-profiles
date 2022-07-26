using ClientWebAppService.PartnerProfile.Business.Models;
using CXI.Contracts.PartnerProfile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.Business
{
    /// <summary>
    /// Provides business flows around partner profiles.
    /// </summary>
    public interface IPartnerProfileService
    {
        /// <summary>
        /// Craete new profile.
        /// </summary>
        Task<PartnerProfileDto> CreateProfileAsync(PartnerProfileCreationModel creationModel);

        /// <summary>
        /// Get profile by id.
        /// </summary>
        Task<PartnerProfileDto> GetByIdAsync(string partnerId);

        /// <summary>
        /// Get all partner profiles.
        /// </summary>
        Task<IEnumerable<PartnerProfileDto>> GetPartnerProfilesAsync();

        /// <summary>
        /// Update specified partner profile.
        /// </summary>
        Task UpdateProfileAsync(string partnerId, PartnerProfileUpdateModel updateModel);

        /// <summary>
        /// Fetches active partners by specified POS type.
        /// </summary>
        Task<IEnumerable<PosTypePartnerDto>> GetActivePartnersByPosTypeAsync(string posType);

        /// <summary>
        /// Fetches active partners by specified POS type.
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        Task<List<string>> SearchPartnerIdsByActiveStateAsync(bool? active);

        /// <summary>
        /// Gets PartnerProfiles Paginated records
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<PartnerProfilePaginatedDto> GetPartnerProfilesPaginatedAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Update PartnerSubscription Async
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task UpdatePartnerSubscriptionAsync(string partnerId, SubscriptionUpdateModel model);

        /// <summary>
        /// Complete partner on-boarding
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task CompletePartnerOnBoardingAsync(string partnerId);

        /// <summary>
        /// Bulk Update PartnerSubscription Async
        /// </summary>
        /// <param name="subscriptionPartnerIdDtos"></param>
        /// <returns></returns>
        Task UpdatePartnerSubscriptionsAsync(List<SubscriptionBulkUpdateDto> subscriptionBulkUpdateDtos);

        /// <summary>
        /// Sets Partners isActive flag
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        Task SetPartnerActivityStatusAsync(string partnerId, bool isActive);

        /// <summary>
        /// Get the partner profile for the given partner id
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task<PartnerProfileDto?> FindPartnerProfileAsync(string partnerId);
    }
}