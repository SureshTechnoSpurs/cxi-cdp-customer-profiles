using CXI.Contracts.UserProfile.Models;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    /// <summary>
    /// Provides APIs for creating messages to be sent by Email app.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends invitation message to AzureService Bus topic.
        /// </summary>
        Task SendInvitationMessageToAssociateAsync(string email);

        /// <summary>
        /// Sends feedback message to AzureService Bus topic.
        /// </summary>
        Task SendFeedbackMessageToTechSupportAsync(UserFeedbackCreationDto request);
    }
}
