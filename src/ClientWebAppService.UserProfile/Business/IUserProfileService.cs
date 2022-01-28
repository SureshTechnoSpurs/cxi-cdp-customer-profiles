using CXI.Contracts.UserProfile.Models;
using System.Collections.Generic;
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
        Task<UserProfileDto> CreateProfileAsync(UserCreationDto creationModel);

        /// <summary>
        /// Get profile by email.
        /// </summary>
        Task<UserProfileDto> GetByEmailAsync(string email);

        /// <summary>
        /// Get profiles by seach criteria.
        /// </summary>
        Task<IEnumerable<UserProfileDto>> GetUserProfilesAsync(UserProfileSearchDto criteria);

        /// <summary>
        /// Update user profile.
        /// </summary>
        Task<UserProfileDto> UpdateUserProfilesAsync(UserProfileUpdateDto updateDto);
    }
}