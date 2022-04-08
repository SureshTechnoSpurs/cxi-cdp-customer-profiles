using System.Threading.Tasks;
using ClientWebAppService.PosProfile.Models;

namespace ClientWebAppService.PosProfile.Services.Credentials
{
    /// <summary>
    ///     Service for work with provided pos type credentials model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPosCredentialsService<in T>
    {
        public Task<PosCredentialsConfiguration> Process(string partnerId, T posConfigurations);
    }
}