using ClientWebAppService.UserProfile.Business.Validators;
using ClientWebAppService.UserProfile.Core.Exceptions;
using ClientWebAppService.UserProfile.DataAccess;
using CXI.Common.ExceptionHandling.Primitives;
using CXI.Common.Helpers;
using CXI.Common.Models.Pagination;
using CXI.Common.AuditLog;
using CXI.Common.AuditLog.Models;
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
        private readonly IAuditLog _auditLog;
        public UserProfileService(
            IUserProfileRepository userProfileRepository,
            ILogger<UserProfileService> logger,
            IEmailService emailService,
            IAzureADB2CDirectoryManager azureADB2CDirectoryManager,
            IAuditLog auditLog)
        {
            _userProfileRepository = userProfileRepository;
            _logger = logger;
            _emailService = emailService;
            _azureADB2CDirectoryManager = azureADB2CDirectoryManager;
            _auditLog = auditLog;
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> GetByEmailAsync(string email)
        {
            _logger.LogInformation($"Get user profile by email: {email}");

            VerifyHelper.NotEmpty(email, nameof(email));

            var validator = new EmailValidator();
            var validationResult = validator.Validate(email);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(nameof(email), validationResult.Errors.FirstOrDefault().ToString());
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
                    throw new ValidationException(
                        nameof(creationModel.Role),
                        $"Creating a SuperUser is forbidden.");
                }

                _logger.LogInformation($"Creating new user profile for partnerId : {creationModel.PartnerId}");

                var result = await _userProfileRepository.FindOne(
                    x => x.Email == creationModel.Email);

                if (result != null)
                {
                    _logger.LogError(
                        $"CreateProfileAsync - Attempted to create user profile with email ({creationModel.Email}) and partner Id ${creationModel.PartnerId}. User profile already exists.");
                    throw new NotFoundException(
                        $"User profile with email ({creationModel.Email}) already exists.");
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
                _logger.LogError(
                    $"CreateProfileAsync - Attempted to create user profile with {creationModel.Role} role for ${creationModel.PartnerId}, Exception message - {ex.Message}");
                throw;
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<UserProfileAssociateDto>> GetUserProfilesAsync(UserProfileSearchDto criteria)
        {
            _logger.LogInformation($"Retrieving user profiles by search criteria.");

            var userProfileAssociates = new List<UserProfileAssociateDto>();

            var result = await _userProfileRepository.FilterBy(x => x.PartnerId == criteria.PartnerId && x.Role == criteria.Role);

            var useremail = result.Select(x => x.Email).ToList();
            
            if (!useremail.Any())
            {
                foreach (var user in result)
                {
                    var userProfile = new UserProfileAssociateDto(user.PartnerId, user.Email, user.Role, user.InvitationAccepted,  null);

                    userProfileAssociates.Add(userProfile);
                }

                return userProfileAssociates;
            }
            //get audit logs API call by useremail
            var partnerAuditlogs = _auditLog.GetAuditLogByEmails(useremail);

            var auditlogUsers = from i in partnerAuditlogs.Result
                                group i by i.UserEmail into g
                                select g.OrderByDescending(t => t.EventDate).FirstOrDefault();

            var dictAuditlogUsers = new Dictionary<string, DisplayAuditLogsDto>();
            foreach (var item in auditlogUsers)
            {
                dictAuditlogUsers.Add(item.UserEmail, item);
            }

            foreach (var user in result)
            {
                var auditlog = dictAuditlogUsers.ContainsKey(user.Email) ? dictAuditlogUsers[user.Email] : null;
                var userProfile = new UserProfileAssociateDto(user.PartnerId, user.Email, user.Role, user.InvitationAccepted, auditlog == null ? null : auditlog.EventDate);

                userProfileAssociates.Add(userProfile);
            }

            if (!userProfileAssociates.Any())
            {
                _logger.LogInformation($"User profiles were not found for partnerId {criteria.PartnerId}.");
                throw new NotFoundException($"User profiles were not found.");
            }

            return userProfileAssociates;
        }

        ///<inheritdoc/>
        public async Task<UserProfileDto> UpdateUserProfilesAsync(UserProfileUpdateDto updateDto)
        {
            _logger.LogInformation($"Updating user profile for partnerId = {updateDto.PartnerId} with email = {updateDto.Email}.");

            try
            {
                var updatedUser = await _userProfileRepository.UpdateAsync(updateDto.PartnerId, updateDto.Email, updateDto.InvitationAccepted.Value);

                _logger.LogInformation($"Successfully updated user profile for partnerId = {updateDto.PartnerId} with email = {updateDto.Email}.");

                return Map(updatedUser);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    $"UpdateUserProfilesAsync - Attempted to update user profile for partnerId = {updateDto.PartnerId}, Exception message - {exception.Message}");
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
                throw new OwnerDeletionForbiddenException(
                    $"DeleteProfileByEmailAsync. Deleting user with role \"Owner\" with email: {email} is forbidden.");
            }

            await _azureADB2CDirectoryManager.DeleteADB2CAccountByEmailAsync(email);
            await _userProfileRepository.DeleteOne(x => x.Email == email);
        }

        public async Task<Dictionary<string, int>> GetUsersCountByPartners(List<string> partnerIds)
        {
            VerifyHelper.CollectionNotEmpty(partnerIds, nameof(partnerIds));

            var result = (await _userProfileRepository.FilterBy(x => partnerIds.Contains(x.PartnerId))).ToList();

            return partnerIds.ToDictionary(partnerId => partnerId, partnerId => result.Count(r => r.PartnerId.Equals(partnerId)));
        }

        private UserProfileDto Map(User profile) =>
            new UserProfileDto(profile.PartnerId, profile.Email, profile.Role, profile.InvitationAccepted);

        ///<inheritdoc cref="GetUserProfilesPaginatedAsync(PaginationRequest)"/>
        public async Task<PaginatedResponse<UserProfileDto>> GetUserProfilesPaginatedAsync(PaginationRequest request)
        {
            VerifyHelper.GreaterThanZero(request.PageIndex, nameof(request.PageIndex));
            VerifyHelper.GreaterThanZero(request.PageSize, nameof(request.PageSize));

            var result = await _userProfileRepository.GetPaginatedList(request);

            VerifyHelper.NotNull(result, nameof(result));

            return MapToPaginatedResponse(result);
        }

        private PaginatedResponse<UserProfileDto> MapToPaginatedResponse(PaginatedResponse<User> model)
        {
            return new PaginatedResponse<UserProfileDto>
            {
                Items = model.Items.Select(Map).ToList(),
                PageIndex = model.PageIndex,
                PageSize = model.PageSize,
                TotalCount = model.TotalCount,
                TotalPages = model.TotalPages
            };
        }

        public async Task<bool> UpdateUserRoleByEmailAsync(UserProfileUpdateRoleDto userProfileUpdateRole)
        {
            _logger.LogInformation($"Update user role for email = {userProfileUpdateRole.Email}.");

            var email = userProfileUpdateRole.Email;
            var role = userProfileUpdateRole.Role;

            VerifyHelper.NotEmpty(email, nameof(email));
            VerifyHelper.NotNull(role, nameof(role));

            var userByEmail = await GetByEmailAsync(email);

            if (userByEmail != null)
            {
                if (role == UserRole.Associate)
                {
                    await ValidateOwner(email, userByEmail.PartnerId);
                }
                await _userProfileRepository.UpdateUserRoleAsync(userProfileUpdateRole);
            }

            _logger.LogInformation($"Successfully updated user role for email = {email}.");

            return true;
        }

        private async Task ValidateOwner(string email, string partnerId)
        {
            var ownerCount = await GetOwnerCount(partnerId, email);

            if (ownerCount <= 0)
            {
                throw new ValidationException("PartnerId", $"Partner ({partnerId}) must always have owner.");
            }
        }

        private async Task<int> GetOwnerCount(string partnerId, string email)
        {
            var users = await GetUserProfilesByPartnerIdAsync(partnerId);

            var ownerUser = users.Where(x => x.Role == UserRole.Owner && x.Email != email).ToList();

            return ownerUser.Count();
        }

        public async Task<IEnumerable<UserProfileDto>> GetUserProfilesByPartnerIdAsync(string partnerId)
        {
            _logger.LogInformation($"Retrieving user profiles by search criteria.");

            var result = await _userProfileRepository.FilterBy(x => x.PartnerId == partnerId);

            var userProfiles = result.ToList();

            if (result == null || !userProfiles.Any())
            {
                _logger.LogInformation($"User profiles were not found for partnerId {partnerId}.");
                throw new NotFoundException($"User profiles were not found.");
            }

            return userProfiles.Select(x => Map(x));
        }

        ///<inheritdoc/>
        public async Task DeleteUserProfilesByPartnerIdAsync(string partnerId)
        {
            _logger.LogInformation($"Deleteing all user profiles for partnerId: {partnerId}");

            VerifyHelper.NotEmptyOrWhiteSpace(partnerId, nameof(partnerId));

            var userEmailsToDelete = await _userProfileRepository.FilterBy(profile => profile.Email, x => x.PartnerId == partnerId);

            foreach (var email in userEmailsToDelete)
            {
                try
                {
                    await _azureADB2CDirectoryManager.DeleteADB2CAccountByEmailAsync(email);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"DeleteUserProfilesByPartnerIdAsync. Failed to delete ADB2C account for user email: {email}. Exception: {ex.Message}.");
                }
            }

            await _userProfileRepository.DeleteMany(x => x.PartnerId == partnerId);
        }

        ///<inheritdoc/>
        public async Task CreateFeedbackEmailAsync(UserFeedbackCreationDto request)
        {
            _logger.LogInformation($"Trigger partner feedback email: {request.Email}");

            VerifyHelper.NotEmpty(request.Email, nameof(request.Email));

            var validator = new EmailValidator();
            var validationResult = validator.Validate(request.Email);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(nameof(request.Email), validationResult.Errors.FirstOrDefault().ToString());
            }

            await _emailService.SendFeedbackMessageToTechSupportAsync(request);
        }
    }
}