using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.ExceptionHandling;
using CXI.Common.Helpers;
using CXI.Contracts.PosProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Controllers
{
    /// <summary>
    /// Api controller for handling create and read requests for POS Profiles
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Route("api/posprofile")]
    [ApiController]
    public class PosProfileController : ControllerBase
    {
        private readonly IPosProfileService _posProfileService;

        public PosProfileController(IPosProfileService posProfileService)
        {
            _posProfileService = posProfileService;
        }

        /// <summary>
        /// Returns POS profile by specified partnerId
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{partnerId}")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> Get([FromRoute] string partnerId)
        {
            var posProfileGetResult = await _posProfileService.FindPosProfileByPartnerIdAsync(partnerId);
            return Ok(posProfileGetResult);
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationModel"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> Post(PosProfileCreationModel posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAndSecretsAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        /// <summary>
        /// DeleteByPartnerId
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpDelete("byPartnerId/{partnerId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteByPartnerId([FromRoute] string partnerId)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId);
            return Ok();
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("search")]
        [ProducesResponseType(typeof(PosProfileSearchDto), 200)]
        public async Task<IActionResult> Search([FromQuery] PosProfileSearchCriteriaModel searchCriteria)
        {
            var posProfileSearchResult = await _posProfileService.GetPosProfilesAsync(searchCriteria);
            return Ok(posProfileSearchResult);
        }

        /// <summary>
        /// Only for M2M - Returns POS profile by specified profile Id
        /// </summary>
        /// <param name="posProfileId"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m/{posProfileId}")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> M2MGet([FromRoute] string posProfileId)
        {
            var posProfileGetResult = await _posProfileService.FindPosProfileByPartnerIdAsync(posProfileId);
            return Ok(posProfileGetResult);
        }

        /// <summary>
        /// Only for M2M - Updates POS profile by specified profile Id
        /// </summary>
        /// <param name="posProfileId"></param>
        /// <param name="profileUpdateModel"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPut("m2m/{posProfileId}")]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> M2MPut([FromRoute] string posProfileId, [FromBody] PosProfileUpdateModel profileUpdateModel)
        {
            await _posProfileService.UpdatePosProfileAsync(posProfileId, profileUpdateModel);
            return Ok();
        }
    }
}