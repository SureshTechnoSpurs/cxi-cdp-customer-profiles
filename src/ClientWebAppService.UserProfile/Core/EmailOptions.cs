using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.Core
{
    /// <summary>
    /// Represents  Email options.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EmailOptions : IEmailOptions
    {
        public string ReceiverEmail { get; set; }

    }
}
