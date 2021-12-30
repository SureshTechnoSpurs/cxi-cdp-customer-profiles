using ClientWebAppService.PosProfile.Models;
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
    }
}
