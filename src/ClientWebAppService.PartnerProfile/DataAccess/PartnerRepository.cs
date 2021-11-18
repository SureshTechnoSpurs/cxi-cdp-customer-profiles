using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql;
using GL.MSA.Core.ResiliencyPolicy;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.DataAccess
{
    /// <summary>
    /// Repository for intercation with <see cref="Partner"/> collection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartnerRepository : BaseMongoRepository<Partner, ObjectId>, IPartnerRepository
    {
        public PartnerRepository(IMongoDbContext dataContext, IResiliencyPolicyProvider policyProvider)
            : base(dataContext, policyProvider)
        { }

        /// <summary>
        /// Update partner with <paramref name="partnerId"/> by new values from <paramref name="updatedPartner"/>
        /// </summary>
        public Task UpdateAsync(string partnerId, Partner updatedPartner)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);

            var updateStrategy =
                Builders<Partner>.Update.Combine(
                    Builders<Partner>.Update.Set(x => x.PartnerName, updatedPartner.PartnerName),
                    Builders<Partner>.Update.Set(x => x.PartnerType, updatedPartner.PartnerType),
                    Builders<Partner>.Update.Set(x => x.Address, updatedPartner.Address),
                    Builders<Partner>.Update.Set(x => x.AmountOfLocations, updatedPartner.AmountOfLocations),
                    Builders<Partner>.Update.Set(x => x.UserProfiles, updatedPartner.UserProfiles));

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }
    }
}
