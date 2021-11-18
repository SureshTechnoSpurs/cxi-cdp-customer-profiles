using CXI.Common.MongoDb;
using MongoDB.Bson;

namespace ClientWebAppService.UserProfile.DataAccess
{
    /// <summary>
    /// Provide db operations for <see cref="User"/> entity.
    /// </summary>
    public interface IUserProfileRepository 
        : IBaseMongoRepository<User, ObjectId>
    { }
}
