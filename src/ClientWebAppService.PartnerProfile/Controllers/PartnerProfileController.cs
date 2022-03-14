using ClientWebAppService.PartnerProfile.Business;
using ClientWebAppService.PartnerProfile.Business.Models;
using CXI.Common.ExceptionHandling;
using CXI.Contracts.PartnerProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ClientWebAppService.PartnerProfile.Core;

namespace ClientWebAppService.PartnerProfile.Controllers
{
    /// <summary>
    /// Provide functionality for read\create profile information.
    /// </summary>
    [Route("api/profile")]
    [Route("api/v{version:apiVersion}/profile")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    [ExcludeFromCodeCoverage]
    public class PartnerProfileController : ControllerBase
    {
        private readonly IPartnerProfileService _partnerProfileService;

        public PartnerProfileController(IPartnerProfileService partnerProfileService)
        {
            _partnerProfileService = partnerProfileService;
        }

        [HttpGet("{partnerId}")]
        [ProducesResponseType(typeof(PartnerProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> GetById([FromRoute] string partnerId)
        {
            var result = await _partnerProfileService.GetByIdAsync(partnerId);

            return Ok(result);
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m")]
        [ProducesResponseType(typeof(PartnerProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> M2MGet()
        {
            var result = await _partnerProfileService.GetPartnerProfilesAsync();

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PartnerProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> Create([FromBody] PartnerProfileCreationModel request)
        {
            var result = await _partnerProfileService.CreateProfileAsync(request);

            return Ok(result);
        }

        [HttpPut("{partnerId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> Update([FromRoute] string partnerId,
                                                [FromBody] PartnerProfileUpdateModel updateModel)
        {
            await _partnerProfileService.UpdateProfileAsync(partnerId, updateModel);

            return Ok();
        }

        [HttpGet("active/{posType}")]
        [ProducesResponseType(typeof(IEnumerable<PosTypePartnerDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        [ProducesResponseType(typeof(ProblemResponse), 424)]
        public async Task<IActionResult> GetActivePartnersByPosType([FromRoute] string posType)
        {
            var result = await _partnerProfileService.GetActivePartnersByPosTypeAsync(posType);

            return Ok(result);
        }
    }
}
