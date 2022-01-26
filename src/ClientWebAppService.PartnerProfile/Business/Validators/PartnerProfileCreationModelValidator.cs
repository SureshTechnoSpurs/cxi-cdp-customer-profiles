using CXI.Contracts.PartnerProfile.Models;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Validators
{
    /// <summary>
    /// Validator for <see cref="PartnerProfileCreationModel"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartnerProfileCreationModelValidator
        : AbstractValidator<PartnerProfileCreationModel>
    {
        public PartnerProfileCreationModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Address).NotEmpty();
        }
    }
}
