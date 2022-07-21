using CXI.Common.Helpers;
using CXI.Common.MongoDb;
using CXI.Contracts.PartnerProfile.Models;
using GL.MSA.Core.NoSql;
using GL.MSA.Core.ResiliencyPolicy;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
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
           
        }

        ///<inheritdoc/>
        public Task CompletePartnerOnBoarding(string partnerId)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);
            var updateStrategy = Builders<Partner>.Update.Set(x => x.IsOnBoarded, true);

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
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
                    Builders<Partner>.Update.Set(x => x.IsActive, updatedPartner.IsActive),
                    Builders<Partner>.Update.Set(x => x.Subscription, updatedPartner.Subscription));

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }

        /// <inheritdoc cref="UpdateSubscriptionAsync(string, Subscription)"/>
        public Task UpdateSubscriptionAsync(string partnerId, Subscription subscription)
        {
            VerifyHelper.NotEmpty(partnerId, nameof(partnerId));
            VerifyHelper.NotNull(subscription, nameof(subscription));

            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);
            var updateStrategy = Builders<Partner>.Update.Set(x => x.Subscription, subscription);

            var policy = GetDefaultPolicy();
            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }

        /// <inheritdoc cref="UpdateSubscriptionsAsync(List<SubscriptionPartnerIdDto>)"/>
        public Task UpdateSubscriptionsAsync(List<SubscriptionPartnerIdDto> subscriptionPartnerIdDtos)
        {
            var bulkUpdateModel = new List<WriteModel<Partner>>();

            foreach (var record in subscriptionPartnerIdDtos)
            {
                var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == record.PartnerId);
                var updateStrategy = Builders<Partner>.Update.Set(x => x.Subscription, record.Subscription);

                var updateOne = new UpdateOneModel<Partner>(filter, updateStrategy) { IsUpsert = true };
                bulkUpdateModel.Add(updateOne);
            }
            var policy = GetDefaultPolicy();
            return policy.ExecuteAsync(() => _collection.BulkWriteAsync(bulkUpdateModel));
        }

        /// <inheritdoc cref="SetStatus(string, bool)"/>
        public Task SetStatus(string partnerId, bool isActive)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);
            var updateStrategy = Builders<Partner>.Update.Set(x => x.IsActive, isActive);

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }
    }
}
