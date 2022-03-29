using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClientWebAppService.UserProfile.Core.Exceptions;

namespace ClientWebAppService.UserProfile.Business
{
    ///<inheritdoc cref="IAzureADB2CDirectoryManager"/>
    public class AzureADB2CDirectoryManager : IAzureADB2CDirectoryManager
    {
        private readonly ILogger<AzureADB2CDirectoryManager> _logger;
        private readonly IAzureADB2CDirectoryRepository _b2cDirectoryRepository;

        public AzureADB2CDirectoryManager(IAzureADB2CDirectoryRepository aDB2CDirectoryRepository,
            ILogger<AzureADB2CDirectoryManager> logger)
        {
            _logger = logger;
            _b2cDirectoryRepository = aDB2CDirectoryRepository;
        }

        ///<inheritdoc/>
        public async Task DeleteADB2CAccountByEmailAsync(string email)
        {
            try
            {
                var result = await _b2cDirectoryRepository.GetDirectoryUsersByEmail(email);

                if (result.Count == 0)
                {
                    _logger.LogError($"Azure ADB2C directory doesn't contain accounts with the email address( {email} ).)");
                    return;
                }

                if (result.Count > 1)
                {
                    _logger.LogError($"Multiple accounts with the same email address ( {email} ) detected.)");
                    throw new MultipleADB2CAccountsDetectedException($"Multiple accounts with the same email address( {email} ) detected.");
                }

                var userId = result[0].Id;

                await _b2cDirectoryRepository.DeleteDirectoryUserByIdAsync(userId);
            }
            catch (Exception exception)
            {
                _logger.LogError($"DeleteADB2CAccountByEmailAsync - Failed to delete user account with email {email} from Azure ADB2C directory. Exception message - {exception.Message}");
                throw;
            }
        }

       
    }
}
