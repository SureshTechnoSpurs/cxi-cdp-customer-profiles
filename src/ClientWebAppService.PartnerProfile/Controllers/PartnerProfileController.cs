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
using CXI.Common.Helpers;

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
        [ProducesResponseType(typeof(IEnumerable<PartnerProfileDto>), 200)]
        [ProducesResponseType(typeof(PartnerProfilePaginatedDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> M2MGet(int? pageIndex, int? pageSize)
        {
            if (!pageIndex.HasValue && !pageSize.HasValue)
            {
                var result = await _partnerProfileService.GetPartnerProfilesAsync();
                return Ok(result);
            }

            VerifyHelper.GreaterThanZero(pageIndex, nameof(pageIndex));
            VerifyHelper.GreaterThanZero(pageSize, nameof(pageSize));

            var paginatedResult =
                await _partnerProfileService.GetPartnerProfilesPaginatedAsync(pageIndex.Value, pageSize.Value);

            return Ok(paginatedResult);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PartnerProfileDto>), 200)]
        [ProducesResponseType(typeof(PartnerProfilePaginatedDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> GetPaginated(int? pageIndex, int? pageSize)
        {
            if (!pageIndex.HasValue && !pageSize.HasValue)
            {
                var result = await _partnerProfileService.GetPartnerProfilesAsync();
                return Ok(result);
            }

            VerifyHelper.GreaterThanZero(pageIndex, nameof(pageIndex));
            VerifyHelper.GreaterThanZero(pageSize, nameof(pageSize));

            var paginatedResult =
                await _partnerProfileService.GetPartnerProfilesPaginatedAsync(pageIndex.Value, pageSize.Value);

            return Ok(paginatedResult);
        }

        [HttpPost("{partnerId}/complete")]
        [ProducesResponseType(typeof(PartnerProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> CompleteOnBoarding(string partnerId)
        {
            await _partnerProfileService.CompletePartnerOnBoardingAsync(partnerId);

            return Accepted();
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

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("search/{active?}")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<IActionResult> SearchPartnerIdsByActiveStateAsync([FromRoute] bool? active = null)
        {
            var result = await _partnerProfileService.SearchPartnerIdsByActiveStateAsync(active);

            return Ok(result);
        }

        [HttpPut("{partnerId}/subscription")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateSubscription(
            [FromRoute] string partnerId,
            [FromBody] SubscriptionUpdateModel model)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));
            VerifyHelper.NotNull(model, nameof(model));

            await _partnerProfileService.UpdatePartnerSubscriptionAsync(partnerId, model);
            return Ok();
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPut("m2m/subscriptions")]
        [ProducesResponseType(typeof(List<SubscriptionBulkUpdateDto>), 200)]
        public async Task<IActionResult> UpdatePartnerSubscriptionsAsync([FromBody] List<SubscriptionBulkUpdateDto> subscriptionBulkUpdateDtos)
        {
            await _partnerProfileService.UpdatePartnerSubscriptionsAsync(subscriptionBulkUpdateDtos);

            return Ok();
        }

        /// <summary>
        /// Sets partner IsActive flag
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [HttpPut("{partnerId}/active")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SetActivityStatus([FromRoute] string partnerId, [FromBody] bool value)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));

            await _partnerProfileService.SetPartnerActivityStatusAsync(partnerId, value);
            return Ok();
        }

        /// <summary>
        /// Sets partner IsActive flag
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPut("m2m/{partnerId}/active")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> M2MSetActivityStatus([FromRoute] string partnerId, [FromBody] bool value)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));

            await _partnerProfileService.SetPartnerActivityStatusAsync(partnerId, value);
            return Ok();
        }

        [HttpGet("find/{partnerId}")]
        [ProducesResponseType(typeof(PartnerProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> FindPartnerProfile([FromRoute] string partnerId)
        {
            var result = await _partnerProfileService.FindPartnerProfileAsync(partnerId);

            return Ok(result);
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m/{partnerId}")]
        [ProducesResponseType(typeof(PartnerProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> M2MGetById([FromRoute] string partnerId)
        {
            var result = await _partnerProfileService.GetByIdAsync(partnerId);

            return Ok(result);
        }
    }
}
