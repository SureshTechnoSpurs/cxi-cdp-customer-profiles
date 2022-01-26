using ClientWebAppService.UserProfile.Business;
using CXI.Common.ExceptionHandling;
using CXI.Contracts.UserProfile.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Controllers
{
    /// <summary>
    /// Provide functionality for read\create profile information.
    /// </summary>
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
        public async Task<IActionResult> Create([FromBody] UserCreationModel request)
        {
            var result = await _userProfileService.CreateProfileAsync(request);

            return Ok(result);
        }
    }
}
