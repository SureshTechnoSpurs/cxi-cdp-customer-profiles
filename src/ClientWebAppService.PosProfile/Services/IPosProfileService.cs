using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CXI.Contracts.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.Create;

namespace ClientWebAppService.PosProfile.Services
{
    /// <summary>
    /// Contains get and create operations for POS Profiles
    /// </summary>
    public interface IPosProfileService
    {
        /// <summary>
        /// Creates POS Profile for particular partner as combination of record in MongoDB
        /// and a key vault record
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        Task<PosProfileDto> CreatePosProfileAndSecretsAsync<T>(PosProfileCreationModel<T> posProfileCreationDto)
            where T : IPosCredentialsConfigurationBaseDto;

        /// <summary>
        /// Returns POS Profile by <paramref name="partnerId"/>\
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [Obsolete("Partner may have more than 1 Pos. Use GetPosProfilesByPartnerId")]
        Task<PosProfileDto> FindPosProfileByPartnerIdAsync(string partnerId);

        /// <summary>
        /// Returns POS Profiles for <paramref name="partnerId"/>\
        /// </summary>
        /// <param name="partnerId"></param>
        Task<IEnumerable<PosProfileDto>> GetPosProfilesByPartnerId(string partnerId);

        /// <summary>
        /// Fetches POS profiles specified by search criteria
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PosProfileSearchDto>> GetPosProfilesAsync(PosProfileSearchCriteriaModel searchCriteria);

        /// <summary>
        /// Performs update operation for POS Profile
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        Task UpdatePosProfileAsync(string partnerId, PosProfileUpdateModel updateModel);

        /// <summary>
        /// Removes posProfile record and keyVault secrets related to particular partner
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task DeletePosProfileAndSecretsAsync(string partnerId);

        /// <summary>
        /// Obtains accessToken from keyVaul for particalar PartnerId
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task<string> GetAccesTokenForPartner(string partnerId);

        /// <summary>
        /// Gets PosProfile by specific MerchantId
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        Task<PosProfileDto> GetByMerchantId(string merchantId);

        /// <summary>
        /// Gets Pos Profiles by partnerIds
        /// </summary>
        /// <param name="partnerIds"></param>
        /// <returns></returns>
        Task<IEnumerable<PosProfileDto>> GetPosProfilesByPartnerIdsAsync(IEnumerable<string> partnerIds);
    }
}