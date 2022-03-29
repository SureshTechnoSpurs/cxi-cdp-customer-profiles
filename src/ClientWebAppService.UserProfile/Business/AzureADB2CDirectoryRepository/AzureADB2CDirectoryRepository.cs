using Azure.Identity;
using ClientWebAppService.UserProfile.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    ///<inheritdoc cref="IAzureADB2CDirectoryRepository"/>
    [ExcludeFromCodeCoverage]
    public class AzureADB2CDirectoryRepository : IAzureADB2CDirectoryRepository
    {
        private readonly AdB2CMicrosoftGraphOptions _b2cMicrosoftGraphOptions;
        private readonly GraphServiceClient _graphClient;

        public AzureADB2CDirectoryRepository(ILogger<AzureADB2CDirectoryManager> logger,
            IOptions<AdB2CMicrosoftGraphOptions> b2cMicrosoftGraphOptions)
        {
            _b2cMicrosoftGraphOptions = b2cMicrosoftGraphOptions.Value;
            _graphClient = CreateGraphApiClient();
        }

        ///<inheritdoc/>
        public async Task<IGraphServiceUsersCollectionPage> GetDirectoryUsersByEmail(string email)
        {
            return await _graphClient.Users
                  .Request()
                  .Filter($"identities/any(c:c/issuerAssignedId eq '{email}' and c/issuer eq '{_b2cMicrosoftGraphOptions.Domain}')")
                  .Select(e => new
                  {
                      e.Id,
                      e.Identities
                  })
                  .GetAsync();
        }

        ///<inheritdoc/>
        public async Task DeleteDirectoryUserByIdAsync(string userId)
        {
            await _graphClient.Users[userId]
                 .Request()
                 .DeleteAsync();
        }

        private GraphServiceClient CreateGraphApiClient()
        {
            var clientSecretCredential = new ClientSecretCredential(_b2cMicrosoftGraphOptions.Domain,
                _b2cMicrosoftGraphOptions.MicrosoftGraphAppId, _b2cMicrosoftGraphOptions.MicrosoftGraphAppClientSercret);
            return new GraphServiceClient(clientSecretCredential);
        }
    }
}
