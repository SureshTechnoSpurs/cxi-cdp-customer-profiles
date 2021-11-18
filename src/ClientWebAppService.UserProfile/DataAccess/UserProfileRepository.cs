using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql;
using GL.MSA.Core.ResiliencyPolicy;
using MongoDB.Bson;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.DataAccess
{
    ///<inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class UserProfileRepository 
        : BaseMongoRepository<User, ObjectId>, IUserProfileRepository
    {
        public UserProfileRepository(IMongoDbContext dataContext, IResiliencyPolicyProvider policyProvider)
            : base(dataContext, policyProvider)
        { }
    }
}
