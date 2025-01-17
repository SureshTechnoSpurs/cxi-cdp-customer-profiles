﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Services;
using CXI.Common.ExceptionHandling;
using CXI.Common.Helpers;
using CXI.Contracts.PosProfile.Models;
using CXI.Contracts.PosProfile.Models.Create;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClientWebAppService.PosProfile.Controllers
{
    /// <summary>
    /// Api controller for handling create and read requests for POS Profiles
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Route("api/posprofile")]
    [Route("api/v{version:apiVersion}/posprofile")]
    [ApiVersion("1.0")]
    [ApiController]
    public class PosProfileController : ControllerBase
    {
        private readonly IPosProfileService _posProfileService;
        private readonly ILogger<PosProfileController> _logger;

        public PosProfileController(IPosProfileService posProfileService, ILogger<PosProfileController> logger)
        {
            _posProfileService = posProfileService;
            _logger = logger;
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
            var posProfileGetResult = await _posProfileService.GetPosProfileByPartnerId(partnerId);
            return Ok(posProfileGetResult);
        }

        /// <summary>
        /// Gets PosProfile by specific merchantId
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m/merchantId/{merchantId}")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> GetByMerchantId([FromRoute] string merchantId)
        {
            VerifyHelper.NotEmpty(merchantId, nameof(merchantId));

            var result = await _posProfileService.GetByMerchantId(merchantId);
            return Ok(result);
        }

        /// <summary>
        /// Returns accessToken for specified partnerId
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{partnerId}/accessToken")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetAccessToken(string partnerId)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));

            var accessToken = await _posProfileService.GetAccesTokenForPartner(partnerId);
            return Ok(accessToken);
        }

        /// <summary>
        /// DeleteByPartnerId and posType
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("partnerId/{partnerId}/posType/{posType}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteByPartnerId([FromRoute] string partnerId, [FromRoute]string posType)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));
            VerifyHelper.NotEmpty(posType, nameof(posType));

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId, posType);
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
        /// <returns></returns>`
        [Obsolete("Partner may have more than 1 Pos. Use M2MGetPosProfiles")]
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m/{posProfileId}")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> M2MGet([FromRoute] string posProfileId)
        {
            var posProfileGetResult = await _posProfileService.GetPosProfileByPartnerId(posProfileId);
            return Ok(posProfileGetResult);
        }

        /// <summary>
        /// Only for M2M - Returns POS profiles by specified partner Id
        /// </summary>
        /// <param name="partnerId"></param>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpGet("m2m/{partnerId}/profile")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        public async Task<IActionResult> M2MGetPosProfile([FromRoute] string partnerId)
        {
            var result = await _posProfileService.GetPosProfileByPartnerId(partnerId);

            return Ok(result);
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

        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpDelete("m2m/partnerId/{partnerId}/posType/{posType}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> M2mDeleteByPartnerId([FromRoute] string partnerId, [FromRoute] string posType)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));
            VerifyHelper.NotEmpty(posType, nameof(posType));

            await _posProfileService.DeletePosProfileAndSecretsAsync(partnerId, posType);
            return Ok();
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationModel{T}"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("m2m/square")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> M2MPostSquare(PosProfileCreationModel<PosCredentialsConfigurationSquareCreationDto> posProfileCreationDto)
        {
            _logger.LogInformation(
                $"Creating new Pos Profile for partnerId = {posProfileCreationDto.PartnerId}, " +
                $"merchantId = {posProfileCreationDto.PosConfigurations?.MerchantId}");

            var posProfileCreateResult = await _posProfileService.CreatePosProfileAndSecretsAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationModel{T}"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("m2m/omnivore")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> M2MPostOmnivore(
            PosProfileCreationModel<PosCredentialsConfigurationOmnivoreCreationDto> posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAndSecretsAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationModel{T}"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("m2m/parbrink")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> M2MPostParBrink(
            PosProfileCreationModel<PosCredentialsConfigurationParBrinkCreationDto> posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAndSecretsAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationModel{T}"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("m2m/olo")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> M2MPostOlo(PosProfileCreationModel<PosCredentialsConfigurationOloCreationDto> posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAndSecretsAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        /// <summary>
        /// Creates POS profile based on <see cref="PosProfileCreationModel{T}"/>
        /// </summary>
        /// <param name="posProfileCreationDto"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("m2m/toast")]
        [ProducesResponseType(typeof(PosProfileDto), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> M2MPostToast(PosProfileCreationModel<PosCredentialsConfigurationToastCreationDto> posProfileCreationDto)
        {
            var posProfileCreateResult = await _posProfileService.CreatePosProfileAndSecretsAsync(posProfileCreationDto);
            return Ok(posProfileCreateResult);
        }

        /// <summary>
        /// Gets Pos Profiles by partnerIds
        /// </summary>
        /// <param name="partnerIds"></param>
        /// <returns></returns>
        [Authorize(Policy = Constants.M2MPolicy)]
        [HttpPost("m2m/getbypartnerids")]
        [ProducesResponseType(typeof(IEnumerable<PosProfileDto>), 200)]
        public async Task<IActionResult> M2MGetPosProfilesByPartnerIds(IEnumerable<string> partnerIds)
        {
            VerifyHelper.CollectionNotEmpty(partnerIds, nameof(partnerIds));

            var posProfiles = await _posProfileService.GetPosProfilesByPartnerIdsAsync(partnerIds);
            return Ok(posProfiles);
        }

        [HttpPost("getbypartnerids")]
        [ProducesResponseType(typeof(IEnumerable<PosProfileDto>), 200)]
        public async Task<IActionResult> GetPosProfilesByPartnerIds(IEnumerable<string> partnerIds)
        {
            VerifyHelper.CollectionNotEmpty(partnerIds, nameof(partnerIds));

            var posProfiles = await _posProfileService.GetPosProfilesByPartnerIdsAsync(partnerIds);
            return Ok(posProfiles);
        }
    }
}