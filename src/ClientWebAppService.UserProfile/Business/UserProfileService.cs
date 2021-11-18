using ClientWebAppService.UserProfile.Business.Models;
using ClientWebAppService.UserProfile.Business.Validators;
using ClientWebAppService.UserProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    ///<inheritdoc/>
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(IUserProfileRepository userProfileRepository,
            ILogger<UserProfileService> logger)
        {
            _userProfileRepository = userProfileRepository;
            _logger = logger;
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> GetByEmailAsync(string email)
        {
            var validator = new EmailValidator();
            var validationResult = validator.Validate(email);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(nameof(email), validationResult.Errors.ToString());
            }

            var result = await _userProfileRepository.FindOne(x => x.Email == email);

            return Map(result);
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> CreateProfileAsync(UserCreationModel creationModel)
        {
            try
            {
                _logger.LogInformation($"Creating new UserProfile for partnerId : {creationModel.PartnerId}");

                var newUser = new User
                {
                    Email = creationModel.Email,
                    PartnerId = creationModel.PartnerId,
                    Role = creationModel.Role
                };

                await _userProfileRepository.InsertOne(newUser);

                _logger.LogInformation($"Successfully created user profile for partnerId = {creationModel.PartnerId}");
                return Map(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateProfileAsync - Attempted to create profile for ${creationModel.PartnerId}, Exception message - {ex.Message}");
                throw;
            }
        }

        private UserProfileDto Map(User profile) =>
             new UserProfileDto(profile.PartnerId, profile.Email, profile.Role);
    }
}
