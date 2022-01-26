using CXI.Contracts.UserProfile.Models;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    /// <summary>
    /// Provide business flows around users proofiles.
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Create new user profile.
        /// </summary>
        Task<UserProfileDto> CreateProfileAsync(UserCreationModel creationModel);

        /// <summary>
        /// Get profile by email.
        /// </summary>
        Task<UserProfileDto> GetByEmailAsync(string email);
    }
}