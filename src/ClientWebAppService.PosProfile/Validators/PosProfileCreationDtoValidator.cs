using System.Diagnostics.CodeAnalysis;
using ClientWebAppService.PosProfile.Models;
using FluentValidation;

namespace ClientWebAppService.PosProfile.Validators
{
    /// <summary>
    /// Validator for <see cref="PosProfileCreationDto"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PosProfileCreationDtoValidator : AbstractValidator<PosProfileCreationDto>
    {
        public PosProfileCreationDtoValidator()
        {
            RuleFor(p => p.PartnerId).NotEmpty();
            RuleForEach(p => p.PosConfigurations)
                .ChildRules(p =>
                {
                    p.RuleFor(pc => pc.PosType).NotEmpty();
                    p.RuleFor(pc => pc.AccessToken).NotEmpty();
                    p.RuleFor(pc => pc.ExpirationDate).NotEmpty();
                    p.RuleFor(pc => pc.RefreshToken).NotEmpty();
                });
        }
    }
}