﻿using System.Diagnostics.CodeAnalysis;
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
    public class PosProfileController : ControllerBase
    {
        private readonly IPosProfileService _posProfileService;

        public PosProfileController(IPosProfileService posProfileService)
        {
            _posProfileService = posProfileService;
        }

        /// <summary>
        /// Returns POS profile by specified profile Id
        /// </summary>
        /// <param name="posProfileId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{posProfileId}")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> Get([FromRoute] string posProfileId)
        {
            var posProfileGetResult = await _posProfileService.FindPosProfileByPartnerIdAsync(posProfileId);
            return Ok(posProfileGetResult);
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationDto"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> Post(PosProfileCreationDto posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("search")]
        [ProducesResponseType(typeof(PosProfileSearchDto), 200)]
        public async Task<IActionResult> Search([FromQuery]PosProfileSearchCriteria searchCriteria)
        {
            var posProfileSearchResult = await _posProfileService.GetPosProfilesAsync(searchCriteria);
            return Ok(posProfileSearchResult);
        }
    }
}