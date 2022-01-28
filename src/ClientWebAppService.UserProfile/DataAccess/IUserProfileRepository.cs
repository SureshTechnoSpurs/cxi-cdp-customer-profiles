using CXI.Common.MongoDb;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.DataAccess
{
    /// <summary>
    /// Provide db operations for <see cref="User"/> entity.
    /// </summary>
    public interface IUserProfileRepository
       : IBaseMongoRepository<User, ObjectId>
    {
        /// <summary>
        /// Updates userProfile with <paramref name="partnerId"/> and <paramref name="email"/> by new values.
        /// </summary>
        Task<User> UpdateAsync(string partnerId, string email, bool invitationAccepted);
    }
}
