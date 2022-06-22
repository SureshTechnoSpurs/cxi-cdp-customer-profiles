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
    /// Api controller for handling create and read requests for POS Profiles
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/parbrink")]
    [Route("api/v{version:apiVersion}/parbrink")]
    [ApiVersion("1.0")]
    [ExcludeFromCodeCoverage]
    public class ParBrinkController : ControllerBase
    {
        private readonly IKeyVaultReferenceService _keyVaultReferenceService;
        private const string PosType = "parbrink";

        public ParBrinkController(IKeyVaultReferenceService keyVaultReferenceService)
        {
            _keyVaultReferenceService = keyVaultReferenceService;
        }

        /// <summary>
        /// Get the locations based on the partner id
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("locations/{partnerId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ParBrinkKeyReferenceModel), 200)]
        public async Task<IActionResult> GetParBrinkLocations([FromRoute] string partnerId)
        {
            var result = await _keyVaultReferenceService.GetKeyVaultValueByReferenceAsync<ParBrinkKeyReferenceModel>(partnerId, PosType);

            return Ok(result);
        }

        /// <summary>
        /// Save the location to key vault
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="parBrinkKeyReferenceModel"></param>
        /// <returns></returns>
        [HttpPost("locations/{partnerId}")]
        [ProducesResponseType(typeof(ValidationProblemResponse), 400)]
        [ProducesResponseType(202)]
        public async Task<IActionResult> SetParBrinkLocations([FromRoute] string partnerId, [FromBody] ParBrinkKeyReferenceModel parBrinkKeyReferenceModel)
        {
            var result = await _keyVaultReferenceService.SetKeyVaultValueByReferenceAsync<ParBrinkKeyReferenceModel>(partnerId, PosType, parBrinkKeyReferenceModel);

            return Ok(result);
        }
    }
}