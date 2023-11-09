using CXI.Common.MongoDb;
using CXI.Contracts.UserProfile.Models;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.DataAccess
{
    /// <summary>
    /// Provide db operations for <see cref="Feedback"/> entity.
    /// </summary>
    public interface IPartnerFeedbackRepository
       : IBaseMongoRepository<Feedback, ObjectId>
    {
        
    }
}
