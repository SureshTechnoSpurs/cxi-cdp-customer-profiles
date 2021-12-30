using ClientWebAppService.PartnerProfile.Business;
using ClientWebAppService.PartnerProfile.Business.Models;
using ClientWebAppService.PartnerProfile.Core.Exceptions;
using ClientWebAppService.PartnerProfile.Models;
using CXI.Common.ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.Controllers
{
    /// <summary>
    /// Provide functionality for read\create profile information.
    /// </summary>
    [Route("api/profile")]
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
        [ProducesResponseType(typeof(DownstreamServiceRequestFailedException), 424)]
        public async Task<IActionResult> GetActivePartnersByPosType([FromRoute] string posType)
        {
            var result = await _partnerProfileService.GetActivePartnersByPosTypeAsync(posType);

            return Ok(result);
        }
    }
}
