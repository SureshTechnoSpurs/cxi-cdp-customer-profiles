using CXI.Common.MongoDb;
using MongoDB.Bson;

namespace ClientWebAppService.PosProfile.DataAccess
{
    /// <summary>
    /// Represents Data Access Object for POS Profile collection
    /// </summary>
    public interface IPosProfileRepository : IBaseMongoRepository<Models.PosProfile, ObjectId>
    {
    }
}