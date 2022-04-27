using ClientWebAppService.PosProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Contracts.PosProfile.Models;
using Microsoft.Extensions.Logging;
using System;
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

        /// <inheritdoc cref="IPosTypeService.SearchPosTypes(PartnerPosTypesSearchModel)"/>
        public async Task<List<PartnerPosTypesDto>> SearchPosTypes(PartnerPosTypesSearchModel partnerPosTypesSearchModel)
        {
            var partnerIds = partnerPosTypesSearchModel.PartnerIds;
            var posType = partnerPosTypesSearchModel.PosType;

            _logger.LogInformation($"Fetching partner and pos information for Pos Type = {posType}");

            IEnumerable<Models.PosProfile> posProfiles = new List<Models.PosProfile>();

            if(!String.IsNullOrEmpty(posType))
            {
                posProfiles = await _posProfileRepository.FilterBy(posProfile => partnerIds.Contains(posProfile.PartnerId)
                                    && posProfile.PosConfiguration != null && posProfile.PosConfiguration.Any(pcfg => pcfg.PosType == posType));
            }
            else
            {
                posProfiles = await _posProfileRepository.FilterBy(posProfile => partnerIds.Contains(posProfile.PartnerId));
            }

            if (posProfiles == null)
            {
                return new List<PartnerPosTypesDto>();
            }
            var posTypes = new List<PartnerPosTypesDto>();
            foreach (var posProfile in posProfiles)
            {
                posTypes.Add(new PartnerPosTypesDto(
                        posProfile.PartnerId,
                        posProfile.PosConfiguration.Select(x => new string(x.PosType))
                    ));
            }

            _logger.LogInformation($"Suceesfully feched partner and pos information for Pos Type = {posType}");

            return posTypes;
        }
    }
}
