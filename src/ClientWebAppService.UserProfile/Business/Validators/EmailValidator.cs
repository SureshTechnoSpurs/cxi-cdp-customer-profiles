using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Business.Validators
{
    /// <summary>
    /// Validator for email strings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailValidator : AbstractValidator<string>
    {
        public EmailValidator()
        {
            RuleFor(x => x).NotEmpty().EmailAddress().WithMessage(prop => $"{prop} is not a valid email address.");
        }
    }
}
