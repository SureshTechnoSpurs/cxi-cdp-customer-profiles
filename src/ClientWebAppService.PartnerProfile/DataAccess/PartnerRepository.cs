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
        {
            var uniqueParnerIdIndexDefinition = new CreateIndexModel<Partner>(Builders<Partner>.IndexKeys.Ascending(x => x.PartnerId), new CreateIndexOptions { Unique = true });
            var uniqueNameIndexDefinition = new CreateIndexModel<Partner>(Builders<Partner>.IndexKeys.Ascending(x => x.PartnerName), new CreateIndexOptions { Unique = true });
            var uniqueAddressIndexDefinition = new CreateIndexModel<Partner>(Builders<Partner>.IndexKeys.Ascending(x => x.Address), new CreateIndexOptions { Unique = true });
            _collection.Indexes.CreateOne(uniqueParnerIdIndexDefinition);
            _collection.Indexes.CreateOne(uniqueNameIndexDefinition);
            _collection.Indexes.CreateOne(uniqueAddressIndexDefinition);
        }

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
                    Builders<Partner>.Update.Set(x => x.ServiceAgreementAccepted, updatedPartner.ServiceAgreementAccepted),
                    Builders<Partner>.Update.Set(x => x.UserProfiles, updatedPartner.UserProfiles),
                    Builders<Partner>.Update.Set(x => x.IsActive, updatedPartner.IsActive));

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }
    }
}
