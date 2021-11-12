using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;

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
        /// <param name="posProfileDto"></param>
        /// <returns></returns>
        Task<PosProfileDto> CreatePosProfileAsync(PosProfileCreationDto posProfileDto);

        /// <summary>
        /// Returns POS Profile by <paramref name="partnerId"/>\
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task<PosProfileDto> GetPosProfileAsync(string partnerId);
    }
}