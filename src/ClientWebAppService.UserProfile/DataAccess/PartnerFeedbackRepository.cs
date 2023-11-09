using CXI.Common.MongoDb;
using CXI.Contracts.UserProfile.Models;
using GL.MSA.Core.NoSql;
using GL.MSA.Core.ResiliencyPolicy;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ClientWebAppService.UserProfile.DataAccess
{
    ///<inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class PartnerFeedbackRepository
        : BaseMongoRepository<Feedback, ObjectId>, IPartnerFeedbackRepository
    {
        public PartnerFeedbackRepository(IMongoDbContext dataContext, IResiliencyPolicyProvider policyProvider)
            : base(dataContext, policyProvider)
        { }

         
    }
}
