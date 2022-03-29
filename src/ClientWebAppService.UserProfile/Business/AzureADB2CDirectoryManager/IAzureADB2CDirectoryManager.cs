using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    /// <summary>
    /// Provides the APIs for managing user accounts in a Azure ADB2C Directory.
    /// </summary>
    public interface IAzureADB2CDirectoryManager
    {
        /// <summary>
        /// Deletes user's account from Azure ADB2C Directory by email.
        /// </summary>
        Task DeleteADB2CAccountByEmailAsync(string email);
    }
}
