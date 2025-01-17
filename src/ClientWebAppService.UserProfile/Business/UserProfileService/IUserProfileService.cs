﻿using CXI.Common.Models.Pagination;
using CXI.Contracts.UserProfile.Models;
using Microsoft.AspNetCore.Mvc;
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
        Task<IEnumerable<UserProfileAssociateDto>> GetUserProfilesAsync(UserProfileSearchDto criteria);

        /// <summary>
        /// Update user profile.
        /// </summary>
        Task<UserProfileDto> UpdateUserProfilesAsync(UserProfileUpdateDto updateDto);

        /// <summary>
        /// Delete user profile and its account in Azure ADB2C Directory.
        /// </summary>
        Task DeleteProfileByEmailAsync(string email);

        /// <summary>
        /// Get amount of users for each partner
        /// </summary>
        /// <param name="partnerIds"></param>
        /// <returns></returns>
        Task<Dictionary<string, int>> GetUsersCountByPartners(List<string> partnerIds);

        /// <summary>
        /// Gets UserProfiles Paginated records
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<PaginatedResponse<UserProfileDto>> GetUserProfilesPaginatedAsync(PaginationRequest request);

        /// <summary>
        /// Update user role using roleId
        /// </summary>
        /// <param name="userProfileUpdateRole"></param>
        /// <returns></returns>
        Task<bool> UpdateUserRoleByEmailAsync(UserProfileUpdateRoleDto userProfileUpdateRole);

        /// <summary>
        /// Delete all user profiles and their accounts in Azure ADB2C Directory for specified partnerId. 
        /// </summary>
        Task DeleteUserProfilesByPartnerIdAsync(string partnerId);

        /// <summary>
        /// Create Feedback Email.
        /// </summary>
        Task CreateFeedbackEmailAsync(UserFeedbackCreationDto request);

        /// <summary>
        /// Get Partner FeedbackMessage by pagination.
        /// </summary>
        Task<PaginatedResponse<UserFeedbackMessageDto>> GetFeedbackMessageAsync(string partnerId, PaginationRequest paginationRequest);
    }
}