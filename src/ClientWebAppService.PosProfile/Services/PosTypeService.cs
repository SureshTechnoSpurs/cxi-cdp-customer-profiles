using ClientWebAppService.PosProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Services
{
    public class PosTypeService : IPosTypeService
    {
        private readonly IPosProfileRepository _posProfileRepository;
        private readonly ILogger<PosTypeService> _logger;

        public PosTypeService(IPosProfileRepository posProfileRepository, ILogger<PosTypeService> logger)
        {
            _posProfileRepository = posProfileRepository;
            _logger = logger;
        }

        /// <inheritdoc cref="IPosTypeService.GetPosProfileIdsByPosTypeAsync"/>
        public async Task<IEnumerable<string>> GetPosProfileIdsByPosTypeAsync(string posType)
        {
            _logger.LogInformation($"Fetching parnters for Pos Type = {posType}");

            var posProfiles = await _posProfileRepository.FilterBy<string>(posProfile => posProfile.PartnerId,
                profile => profile.PosConfiguration != null &&
                profile.PosConfiguration.Any(pcfg => pcfg.PosType == posType));            

            if (posProfiles == null || !posProfiles.Any())
            {
                _logger.LogInformation($"Pos profiles not found for POS Type = {posType}");
                throw new NotFoundException($"Pos profiles not found for POS Type = {posType}");
            }

            _logger.LogInformation($"Successfully fetched parnters for Pos Type = {posType}");

            return posProfiles;
        }
    }
}
