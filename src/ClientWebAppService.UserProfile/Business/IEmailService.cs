using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.Business
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends invitation message to AzureService Bus topic.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task SendInvitationMessageToAssociateAsync(string email);
    }
}
