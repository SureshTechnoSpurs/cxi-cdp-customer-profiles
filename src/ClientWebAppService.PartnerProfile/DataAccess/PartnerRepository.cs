using CXI.Common.Helpers;
using CXI.Common.MongoDb;
using CXI.Contracts.PartnerProfile.Models;
using GL.MSA.Core.NoSql;
using GL.MSA.Core.ResiliencyPolicy;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                    Builders<Partner>.Update.Set(x => x.Subscription, updatedPartner.Subscription),
                    Builders<Partner>.Update.Set(x => x.ServiceAgreementVersion, updatedPartner.ServiceAgreementVersion),
                    Builders<Partner>.Update.Set(x => x.ServiceAgreementAcceptedDate, updatedPartner.ServiceAgreementAcceptedDate));

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
        public Task UpdateSubscriptionsAsync(IEnumerable<SubscriptionBulkUpdateDto> subscriptionBulkUpdateDtos)
        {
            var bulkUpdateModel = new List<WriteModel<Partner>>();

            foreach (var dto in subscriptionBulkUpdateDtos)
            {
                var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == dto.PartnerId);
                var updateStrategy = Builders<Partner>.Update.Combine(
                    Builders<Partner>.Update.Set(x => x.Subscription, dto.Subscription),
                    Builders<Partner>.Update.Set(x => x.IsActive, dto.IsActive));

                var updateOne = new UpdateOneModel<Partner>(filter, updateStrategy) { IsUpsert = true };
                bulkUpdateModel.Add(updateOne);
            }
            var policy = GetDefaultPolicy();
            return policy.ExecuteAsync(() => _collection.BulkWriteAsync(bulkUpdateModel));
        }

        /// <inheritdoc cref="SetStatus(string, bool)"/>
        public Task SetActivityStatus(string partnerId, bool value)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);
            var updateStrategy = Builders<Partner>.Update.Set(x => x.IsActive, value);

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }

        /// <summary>
        /// Update partner with <paramref name="partnerId"/> by new values from <paramref name="updatedPartner"/>
        /// </summary>
        public Task UpdateProcessConfigAsync(string partnerId, Partner updatedPartner)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);

            var updateStrategy =
                Builders<Partner>.Update.Combine(
                    Builders<Partner>.Update.Set(x => x.SyntheticGenerateFlag, updatedPartner.SyntheticGenerateFlag),
                    Builders<Partner>.Update.Set(x => x.UiEnableFlag, updatedPartner.UiEnableFlag),
                    Builders<Partner>.Update.Set(x => x.DemogPredictFlag, updatedPartner.DemogPredictFlag),
                    Builders<Partner>.Update.Set(x => x.OverviewDashboardFlag, updatedPartner.OverviewDashboardFlag),
                    Builders<Partner>.Update.Set(x => x.IdentityPhoneFlag, updatedPartner.IdentityPhoneFlag),
                    Builders<Partner>.Update.Set(x => x.IdentityEmailFlag, updatedPartner.IdentityEmailFlag),
                    Builders<Partner>.Update.Set(x => x.IdentityIOSFlag, updatedPartner.IdentityIOSFlag),
                    Builders<Partner>.Update.Set(x => x.IdentityAndroidFlag, updatedPartner.IdentityAndroidFlag));

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }

        /// <summary>
        /// Update partner with <paramref name="partnerId"/> by new values from <paramref name="updatedPartner"/>
        /// </summary>
        public Task UpdateTutorialConfigAsync(string partnerId, Partner updatedPartner)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);

            var updateStrategy =
               Builders<Partner>.Update.Combine(
                   Builders<Partner>.Update.Set(x => x.TutorialEnableFlag, updatedPartner.TutorialEnableFlag));

            var policy = GetDefaultPolicy();

            return policy.ExecuteAsync(() => _collection.UpdateOneAsync(filter, updateStrategy));
        }

        /// <summary>
        /// Get partner with <paramref name="partnerId"/> 
        /// </summary>
        public async Task<List<Partner>> GetPartnerConfigAsync(string partnerId)
        {
            var filter = Builders<Partner>.Filter.Where(x => x.PartnerId == partnerId);

            var projection = Builders<Partner>.Projection.Include(x => x.PartnerId)
                                                                      .Include(x => x.IdentityPhoneFlag)
                                                                      .Include(x => x.IdentityEmailFlag)
                                                                      .Include(x => x.IdentityIOSFlag)
                                                                      .Include(x => x.IdentityAndroidFlag);


            var bsonDocuments = await _collection.Find(filter).Project(projection).ToListAsync();

            var partnerConfig = bsonDocuments.Select(document => BsonSerializer.Deserialize<Partner>(document)).ToList();

            return partnerConfig;

        }
    }
}
