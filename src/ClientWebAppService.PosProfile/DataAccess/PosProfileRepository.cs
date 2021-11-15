using System.Diagnostics.CodeAnalysis;
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
    }
}