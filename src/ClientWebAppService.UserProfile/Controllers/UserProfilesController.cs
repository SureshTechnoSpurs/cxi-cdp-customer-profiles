using ClientWebAppService.UserProfile.Business;
using ClientWebAppService.UserProfile.Core;
using CXI.Common.ExceptionHandling;
using CXI.Contracts.UserProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CXI.Common.Helpers;
using CXI.Common.Models.Pagination;
using FluentValidation.AspNetCore;

namespace ClientWebAppService.UserProfile.Controllers
{
    /// <summary>
    /// Provide functionality for user profiles interaction.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Route("api/profile")]
    [Route("api/v{version:apiVersion}/profile")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfilesController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        [HttpGet("{email}")]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> GetByEmail([FromRoute] string email)
        {
            var result = await _userProfileService.GetByEmailAsync(email);

            return Ok(result);
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m/{email}")]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> GetByEmailM2M([FromRoute] string email)
        {
            var result = await _userProfileService.GetByEmailAsync(email);

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> Create([FromBody] UserCreationDto request)
        {
            var result = await _userProfileService.CreateProfileAsync(request);

            return Ok(result);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<UserProfileAssociateDto>), 200)]
        public async Task<IActionResult> Search([FromQuery] UserProfileSearchDto searchCriteria)
        {
            var userProfileSearchResult = await _userProfileService.GetUserProfilesAsync(searchCriteria);

            return Ok(userProfileSearchResult);
        }

        [HttpPut]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> Update([FromBody] UserProfileUpdateDto updateRequest)
        {
            var result = await _userProfileService.UpdateUserProfilesAsync(updateRequest);

            return Ok(result);
        }

        [HttpDelete("{email}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> DeleteByEmail([FromRoute] string email)
        {
            await _userProfileService.DeleteProfileByEmailAsync(email);

            return Ok();
        }

        [HttpPost("count")]
        [ProducesResponseType(typeof(Dictionary<string, int>), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> GetUsersCountByPartners(List<string> partnerIds)
        {
            VerifyHelper.CollectionNotEmpty(partnerIds, nameof(partnerIds));

            var result = await _userProfileService.GetUsersCountByPartners(partnerIds);
            return Ok(result);
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(PaginatedResponse<UserProfileDto>), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> GetPaginatedUser([FromBody] PaginationRequest request)
        {
            var result = await _userProfileService.GetUserProfilesPaginatedAsync(request);

            return Ok(result);
        }

        [HttpPut("role")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> UpdateUserRoleByEmail([FromBody] UserProfileUpdateRoleDto userProfileUpdateRole)
        {
            var result = await _userProfileService.UpdateUserRoleByEmailAsync(userProfileUpdateRole);

            return Ok(result);
        }

        [HttpDelete("partner/{partnerId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> DeleteByPartnerId([CustomizeValidator(Skip =true)][FromRoute] string partnerId)
        {
            await _userProfileService.DeleteUserProfilesByPartnerIdAsync(partnerId);

            return Ok();
        }
        
        [HttpPost("partner-feedback-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> CreateFeedbackEmail([FromBody] UserFeedbackCreationDto request)
        {
             await _userProfileService.CreateFeedbackEmailAsync(request);

            return Ok();
        }
    }
}

