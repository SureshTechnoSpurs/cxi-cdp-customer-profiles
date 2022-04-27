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
    }
}