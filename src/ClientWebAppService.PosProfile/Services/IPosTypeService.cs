using ClientWebAppService.PosProfile.Models;
using CXI.Contracts.PosProfile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services
{
    public interface IPosTypeService
    {
        /// <summary>
        /// Fetches POS partner IDs by POS type.
        /// </summary>
        Task<IEnumerable<string>> GetPosProfileIdsByPosTypeAsync(string posType);
        /// <summary>
        /// Fetches POS type by partner IDs.
        /// </summary>
        /// <param name="partnerIds"></param>
        /// <param name="posType"></param>
        /// <returns></returns>
        Task<List<PosTypePartnerDto>> GetPosTypeByPartnerIdsAsync(PosTypeActivePartnerModel posTypeActivePartner);
    }
}
