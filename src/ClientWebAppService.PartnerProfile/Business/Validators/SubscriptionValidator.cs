using CXI.Contracts.PartnerProfile.Models;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Business.Validators
{
    /// <summary>
    /// Validator for <see cref="Subscription"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SubscriptionValidator : AbstractValidator<Subscription>
    {
        public SubscriptionValidator()
        {
            When(x => x.LastBilledDate.HasValue || x.State.HasValue, () =>
            {
               RuleFor(x => x.SubscriptionId).NotEmpty().NotNull();
            });
            When(x => !string.IsNullOrEmpty(x.SubscriptionId) || x.LastBilledDate.HasValue, () =>
             {
                 RuleFor(x => x.State).NotEmpty().IsInEnum().NotNull();
             });
        }
    }
}
