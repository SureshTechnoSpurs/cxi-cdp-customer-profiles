using System.Diagnostics.CodeAnalysis;
using CXI.Contracts.PosProfile.Models.Create;
using FluentValidation;

namespace ClientWebAppService.PosProfile.Validators
{
    /// <summary>
    /// Validator for <see cref="PosProfileCreationModel"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PosProfileCreationSquareDtoValidator : AbstractValidator<PosProfileCreationModel<PosCredentialsConfigurationSquareCreationDto>>
    {
        public PosProfileCreationSquareDtoValidator()
        {
            RuleFor(p => p.PartnerId).NotEmpty();
            RuleFor(p => p.PosConfigurations)
                .ChildRules(
                    p =>
                    {
                        p.RuleFor(pc => pc.PosType).NotEmpty();
                        p.RuleFor(pc => pc.AccessToken).NotEmpty();
                        p.RuleFor(pc => pc.ExpirationDate).NotEmpty();
                        p.RuleFor(pc => pc.RefreshToken).NotEmpty();
                    });
        }
    }

    [ExcludeFromCodeCoverage]
    public class PosProfileCreationOmnivoreDtoValidator : AbstractValidator<PosProfileCreationModel<PosCredentialsConfigurationOmnivoreCreationDto>>
    {
        public PosProfileCreationOmnivoreDtoValidator()
        {
            RuleFor(p => p.PartnerId).NotEmpty();
            RuleFor(p => p.PosConfigurations)
                .ChildRules(p => { p.RuleFor(pc => pc.PosType).NotEmpty(); });
        }
    }
}