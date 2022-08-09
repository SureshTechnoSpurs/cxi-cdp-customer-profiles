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
    public class UserProfileRepository 
        : BaseMongoRepository<User, ObjectId>, IUserProfileRepository
    {
        public UserProfileRepository(IMongoDbContext dataContext, IResiliencyPolicyProvider policyProvider)
            : base(dataContext, policyProvider)
        { }

        ///<inheritdoc/>
        public async Task<User> UpdateAsync(string partnerId, string email, bool invitationAccepted)
        {
            var filter = Builders<User>.Filter.Where(x => x.PartnerId == partnerId &&
                x.Email == email);

            var updateStrategy =
                    Builders<User>.Update.Set(x => x.InvitationAccepted,
                        invitationAccepted);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var policy = GetDefaultPolicy();

            return await policy.ExecuteAsync(async () => await _collection.FindOneAndUpdateAsync(filter, updateStrategy, options));
        }

        ///<inheritdoc cref="UpdateUserRoleAsync(UserProfileUpdateRoleDto)"/>
        public async Task<User> UpdateUserRoleAsync(UserProfileUpdateRoleDto userProfileUpdateRole)
        {
            var filter = Builders<User>.Filter.Where(x => x.Email == userProfileUpdateRole.Email);

            var updateStrategy =
                    Builders<User>.Update.Set(x => x.Role,
                        userProfileUpdateRole.Role);

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var policy = GetDefaultPolicy();

            return await policy.ExecuteAsync(async () => await _collection.FindOneAndUpdateAsync(filter, updateStrategy, options));
        }
    }
}
