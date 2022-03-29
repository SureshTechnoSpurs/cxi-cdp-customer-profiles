using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    /// <summary>
    /// Azure ADB2C Directory Repository.
    /// </summary>
    public interface IAzureADB2CDirectoryRepository
    {
        /// <summary>
        /// Get all users in directory with specified email.
        /// </summary>
        public Task<IGraphServiceUsersCollectionPage> GetDirectoryUsersByEmail(string email);

        /// <summary>
        /// Delete Directory user by userId.
        /// </summary>
        public Task DeleteDirectoryUserByIdAsync(string userId);
    }
}
