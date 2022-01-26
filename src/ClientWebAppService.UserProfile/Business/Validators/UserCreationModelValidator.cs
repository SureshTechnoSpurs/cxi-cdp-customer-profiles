using CXI.Contracts.UserProfile.Models;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Business.Validators
{
    /// <summary>
    /// Validator for <see cref="UserCreationModel"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserCreationModelValidator : AbstractValidator<UserCreationModel>
    {
        public UserCreationModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.PartnerId).NotEmpty();
            RuleFor(x => x.Role).NotEmpty();
        }
    }
}
