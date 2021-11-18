using CXI.Common.MongoDb;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.DataAccess
{
    ///<inheritdoc/>
    public interface IPartnerRepository :
        IBaseMongoRepository<Partner, ObjectId>
    {
        Task UpdateAsync(string partnerId, Partner updatedPartner);
    }
}
