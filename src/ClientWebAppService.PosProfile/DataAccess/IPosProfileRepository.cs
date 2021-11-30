using System.Threading.Tasks;
using CXI.Common.MongoDb;
using MongoDB.Bson;

namespace ClientWebAppService.PosProfile.DataAccess
{
    /// <summary>
    /// Represents Data Access Object for POS Profile collection
    /// </summary>
    public interface IPosProfileRepository : IBaseMongoRepository<Models.PosProfile, ObjectId>
    {
        /// <summary>
        /// Performs update operation for <paramref name="posProfile"/>
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="posProfile"></param>
        /// <returns></returns>
        Task UpdateAsync(string partnerId, Models.PosProfile posProfile);
    }
}