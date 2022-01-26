using CXI.Contracts.PartnerProfile.Models;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Validators
{
    /// <summary>
    /// Validator for <see cref="UpdatePartnerProfileUsersModel"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartnerProfileUpdateModelValidator
        : AbstractValidator<PartnerProfileUpdateModel>
    {
        public PartnerProfileUpdateModelValidator()
        {
            RuleFor(x => x.PartnerName).NotEmpty();
            RuleFor(x => x.Address).NotEmpty();
            RuleFor(x => x.PartnerType).NotEmpty();
            RuleFor(x => x.UserProfileEmails).NotNull().ForEach(x => x.NotEmpty().EmailAddress());
        }
    }
}
