using ClientWebAppService.UserProfile.Business;
using CXI.Common.ExceptionHandling;
using CXI.Contracts.UserProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Controllers
{
    /// <summary>
    /// Provide functionality for user profiles interaction.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Route("api/profile")]
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

        [HttpPost]
        [ProducesResponseType(typeof(UserProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        public async Task<IActionResult> Create([FromBody] UserCreationDto request)
        {
            var result = await _userProfileService.CreateProfileAsync(request);

            return Ok(result);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), 200)]
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
    }
}

