using ClientWebAppService.PosProfile.Services;
using CXI.Common.ExceptionHandling;
using CXI.Contracts.PosProfile.Models.PosKeyReference;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.PosProfile.Controllers
{
    /// <summary>
    /// Api controller for handling create and read requests for olo POS
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/olo")]
    [Route("api/v{version:apiVersion}/olo")]
    [ApiVersion("1.0")]
    [ExcludeFromCodeCoverage]
    public class OloController : ControllerBase
    {
        private readonly IKeyVaultReferenceService _keyVaultReferenceService;
        private const string PosType = "olo";

        public OloController(IKeyVaultReferenceService keyVaultReferenceService)
        {
            _keyVaultReferenceService = keyVaultReferenceService;
        }

        /// <summary>
        /// Get the api key based on the partner id
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("apikey/{partnerId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(OloKeyReferenceModel), 200)]
        public async Task<IActionResult> GetOloApiKey([FromRoute] string partnerId)
        {
            var result = await _keyVaultReferenceService.GetKeyVaultValueByReferenceAsync<OloKeyReferenceModel>(partnerId, PosType);
            return Ok(result);
        }

        /// <summary>
        /// Save the api key to key vault
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="oloKeyReferenceModel"></param>
        /// <returns></returns>
        [HttpPost("apikey/{partnerId}")]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        [ProducesResponseType(202)]
        public async Task<IActionResult> SetOloApiKey([FromRoute] string partnerId, [FromBody] OloKeyReferenceModel oloKeyReferenceModel)
        {
            var result = await _keyVaultReferenceService.SetKeyVaultValueByReferenceAsync<OloKeyReferenceModel>(partnerId, PosType, oloKeyReferenceModel);

            return Ok(result);
        }
    }
}