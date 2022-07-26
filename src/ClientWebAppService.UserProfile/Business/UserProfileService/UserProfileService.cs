using ClientWebAppService.UserProfile.Business.Validators;
using ClientWebAppService.UserProfile.Core.Exceptions;
using ClientWebAppService.UserProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Contracts.UserProfile.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    ///<inheritdoc/>
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IEmailService _emailService;
        private readonly IAzureADB2CDirectoryManager _azureADB2CDirectoryManager;

        public UserProfileService(
            IUserProfileRepository userProfileRepository,
            ILogger<UserProfileService> logger,
            IEmailService emailService,
            IAzureADB2CDirectoryManager azureADB2CDirectoryManager)
        {
            _userProfileRepository = userProfileRepository;
            _logger = logger;
            _emailService = emailService;
            _azureADB2CDirectoryManager = azureADB2CDirectoryManager;
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> GetByEmailAsync(string email)
        {
            _logger.LogInformation($"Get user profile by email: {email}");

            var validator = new EmailValidator();
            var validationResult = validator.Validate(email);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(nameof(email), validationResult.Errors.ToString());
            }

            var result = await _userProfileRepository.FindOne(x => x.Email == email);

            return result == null ? throw new NotFoundException($"UserProfile with email: {email} not found.") : Map(result);
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> CreateProfileAsync(UserCreationDto creationModel)
        {
            try
            {
                if (creationModel.Role == UserRole.SuperUser)
                {
                    _logger.LogInformation($"Attempted to create SuperUser for partnerId : {creationModel.PartnerId}");
                    throw new ValidationException(nameof(creationModel.Role),
                       $"Creating a SuperUser is forbidden.");
                }

                _logger.LogInformation($"Creating new user profile for partnerId : {creationModel.PartnerId}");

                var result = await _userProfileRepository.FindOne(x => x.Email == creationModel.Email &&
                    x.PartnerId == creationModel.PartnerId);

                if (result != null)
                {
                    _logger.LogError($"CreateProfileAsync - Attempted to create user profile with email ({creationModel.Email}) and parner Id ${creationModel.PartnerId}");
                    throw new ValidationException(nameof(creationModel.PartnerId),
                        $"User profile with email ({creationModel.Email}) and parnerId ${creationModel.PartnerId} already exists.");
                }

                var newUser = new User
                {
                    Email = creationModel.Email,
                    PartnerId = creationModel.PartnerId,
                    Role = creationModel.Role,
                    InvitationAccepted = creationModel.Role == UserRole.Owner ? true : false
                };

                await _userProfileRepository.InsertOne(newUser);

                if (creationModel.Role == UserRole.Associate)
                {
                    await _emailService.SendInvitationMessageToAssociateAsync(creationModel.Email);
                }

                _logger.LogInformation($"Successfully created user profile with {creationModel.Role} role for partnerId = {creationModel.PartnerId}");
                return Map(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateProfileAsync - Attempted to create user profile with {creationModel.Role} role for ${creationModel.PartnerId}, Exception message - {ex.Message}");
                throw;
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<UserProfileDto>> GetUserProfilesAsync(UserProfileSearchDto criteria)
        {
            _logger.LogInformation($"Retrieving user profiles by search criteria.");

            var result = await _userProfileRepository.FilterBy(profile => Map(profile),
                userProfile => userProfile.PartnerId == criteria.PartnerId &&
                userProfile.Role == criteria.Role);

            var userProfiles = result.ToList();

            if (result == null || !userProfiles.Any())
            {
                _logger.LogInformation($"User profiles were not found for partnerId {criteria.PartnerId}.");
                throw new NotFoundException($"User profiles were not found.");
            }

            return userProfiles;
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> UpdateUserProfilesAsync(UserProfileUpdateDto updateDto)
        {
            _logger.LogInformation($"Updating user profile for partnerId = {updateDto.PartnerId} with email = {updateDto.Email}.");

            try
            {
                var updatedUser = await _userProfileRepository.UpdateAsync(updateDto.PartnerId, updateDto.Email, updateDto.InvitationAccepted);

                _logger.LogInformation($"Successfully updated user profile for partnerId = {updateDto.PartnerId} with email = {updateDto.Email}.");

                return Map(updatedUser);
            }
            catch (Exception exception)
            {
                _logger.LogError($"UpdateUserProfilesAsync - Attempted to update user profile for partnerId = {updateDto.PartnerId}, Exception message - {exception.Message}");
                throw;
            }
        }

        ///<inheritdoc/>
        public async Task DeleteProfileByEmailAsync(string email)
        {
            _logger.LogInformation($"Deleteing user profile with email: {email}");

            var validator = new EmailValidator();
            var validationResult = validator.Validate(email);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(nameof(email), validationResult.Errors.ToString());
            }       

            var userToDelete = await _userProfileRepository.FindOne(x => x.Email == email);

            if (userToDelete == null)
            {
                throw new NotFoundException($"DeleteProfileByEmailAsync. UserProfile with email: {email} not found.");
            }

            if (userToDelete.Role == UserRole.Owner)
            {
                throw new OwnerDeletionForbiddenException($"DeleteProfileByEmailAsync. Deleting user with role \"Owner\" with email: {email} is forbidden.");
            }

            await _azureADB2CDirectoryManager.DeleteADB2CAccountByEmailAsync(email);
            await _userProfileRepository.DeleteOne(x => x.Email == email);

        }

        private UserProfileDto Map(User profile) =>
             new UserProfileDto(profile.PartnerId, profile.Email, profile.Role, profile.InvitationAccepted);
    }
}
