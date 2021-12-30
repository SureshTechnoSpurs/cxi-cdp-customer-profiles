using ClientWebAppService.PartnerProfile.Business.Models;
using ClientWebAppService.PartnerProfile.Models;
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
        /// Update specified partner profile.
        /// </summary>
        Task UpdateProfileAsync(string partnerId, PartnerProfileUpdateModel updateModel);

        /// <summary>
        /// Fetches active partners by specified POS type.
        /// </summary>
        Task<IEnumerable<PosTypePartnerDto>> GetActivePartnersByPosTypeAsync(string posType);
    }
}