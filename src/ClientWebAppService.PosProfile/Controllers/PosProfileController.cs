using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;
using ClientWebAppService.PosProfile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientWebAppService.PosProfile.Controllers
{
    /// <summary>
    /// Api controller for handling create and read requests for POS Profiles
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Route("api/posprofile")]
    [ApiController]
    [Authorize]
    public class PosProfileController : ControllerBase
    {
        private readonly IPosProfileService _posProfileService;

        public PosProfileController(IPosProfileService posProfileService)
        {
            _posProfileService = posProfileService;
        }
        [HttpGet("{posProfileId}")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> Get([FromRoute] string posProfileId)
        {
           var posProfileGetResult = await _posProfileService.GetPosProfileAsync(posProfileId);
            return Ok(posProfileGetResult);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> Post(PosProfileCreationDto posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }
    }
}