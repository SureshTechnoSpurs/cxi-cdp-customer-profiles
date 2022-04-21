using ClientWebAppService.PosProfile.Services;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Contracts.PosProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route("api/postype")]
    [Route("api/v{version:apiVersion}/postype")]
    [ApiVersion("1.0")]
    [ApiController]
    public class PosTypeController : ControllerBase
    {
        private readonly IPosTypeService _posTypeService;

        public PosTypeController(IPosTypeService posTypeService)
        {
            _posTypeService = posTypeService;
        }

        [Authorize]
        [HttpGet("{posType}")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(typeof(NotFoundException), 404)]
        public async Task<IActionResult> GetPosProfileIdsByPosTypeAsync([FromRoute] string posType)
        {
            var posProfileIds = await _posTypeService.GetPosProfileIdsByPosTypeAsync(posType);
            return Ok(posProfileIds);
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("partnerPosType")]
        [ProducesResponseType(typeof(List<PosTypePartnerDto>), 200)]
        [ProducesResponseType(typeof(NotFoundException), 404)]
        public async Task<IActionResult> GetPosTypeByPartnerIdsAsync([FromBody] PosTypeActivePartnerModel posTypeActivePartner)
        {
            var partnerPosTypes = await _posTypeService.GetPosTypeByPartnerIdsAsync(posTypeActivePartner);
            return Ok(partnerPosTypes);
        }
    }
}
