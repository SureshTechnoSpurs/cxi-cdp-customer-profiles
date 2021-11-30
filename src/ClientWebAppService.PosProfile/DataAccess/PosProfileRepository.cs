using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql;
using GL.MSA.Core.ResiliencyPolicy;
using MongoDB.Bson;

namespace ClientWebAppService.PosProfile.DataAccess
{
    ///<inheritdoc cref="IPosProfileRepository"/> 
    [ExcludeFromCodeCoverage]
    public class PosProfileRepository : BaseMongoRepository<Models.PosProfile, ObjectId>, IPosProfileRepository
    {
        public PosProfileRepository(IMongoDbContext dataContext, IResiliencyPolicyProvider policyProvider) : base(dataContext, policyProvider)
        {
        }
        
        /// <summary>
        /// Updates posProfile with <paramref name="partnerId"/> by new values from <paramref name="posProfile"/>
        /// </summary>
        public async Task UpdateAsync(string partnerId, Models.PosProfile posProfile)
        {
            var filter = MongoDB.Driver.Builders<Models.PosProfile>.Filter.Where(x => x.PartnerId == partnerId);

            var updateStrategy =
                MongoDB.Driver.Builders<Models.PosProfile>.Update.Combine(
                    MongoDB.Driver.Builders<Models.PosProfile>.Update.Set(x => x.PosConfiguration,
                        posProfile.PosConfiguration),
                    MongoDB.Driver.Builders<Models.PosProfile>.Update.Set(x => x.IsHistoricalDataIngested,
                        posProfile.IsHistoricalDataIngested));

            var policy = GetDefaultPolicy();

            await policy.ExecuteAsync(async () => await _collection.UpdateOneAsync(filter, updateStrategy));
        }
        
    }
}