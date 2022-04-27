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
        [HttpPost("partnersearch")]
        [ProducesResponseType(typeof(List<PartnerPosTypesDto>), 200)]
        public async Task<IActionResult> SearchPosTypes([FromBody] PartnerPosTypesSearchModel partnerPosTypesSearchModel)
        {
            var partnerPosTypes = await _posTypeService.SearchPosTypes(partnerPosTypesSearchModel);
            return Ok(partnerPosTypes);
        }
    }
}
