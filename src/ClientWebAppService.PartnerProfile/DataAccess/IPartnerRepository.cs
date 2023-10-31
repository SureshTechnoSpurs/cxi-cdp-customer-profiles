using CXI.Common.MongoDb;
using CXI.Contracts.PartnerProfile.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientWebAppService.PartnerProfile.DataAccess
{
    ///<inheritdoc/>
    public interface IPartnerRepository : IBaseMongoRepository<Partner, ObjectId>
    {
        /// <summary>
        /// Complete partner on-boarding 
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        Task CompletePartnerOnBoarding(string partnerId);

        Task UpdateAsync(string partnerId, Partner updatedPartner);

        /// <summary>
        /// Update Subscription Async
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="subscription"></param>
        /// <returns></returns>
        Task UpdateSubscriptionAsync(string partnerId, Subscription subscription);

        /// <summary>
        /// Bulk Update Subscriptions Async
        /// </summary>
        /// <param name="subscriptionPartnerIdDtos"></param>
        /// <returns></returns>
        Task UpdateSubscriptionsAsync(IEnumerable<SubscriptionBulkUpdateDto> subscriptionBulkUpdateDtos);

        /// <summary>
        /// Sets partners IsActive flag
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        Task SetActivityStatus(string partnerId, bool value);

        Task UpdateProcessConfigAsync(string partnerId, Partner updatedPartner);
    }
}
