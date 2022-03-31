using CXI.Contracts.UserProfile.Models;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Business.Validators
{
    /// <summary>
    /// Validator for <see cref="UsersearchModel"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserSearchModelValidator : AbstractValidator<UserProfileSearchDto>
    {
        public UserSearchModelValidator()
        {
            RuleFor(x => x.PartnerId).NotEmpty();
            RuleFor(x => x.Role).IsInEnum();
        }
    }
}
