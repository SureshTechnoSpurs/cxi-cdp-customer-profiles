using CXI.Common.MongoDb;
using CXI.Contracts.PartnerProfile.Models;
using GL.MSA.Core.NoSql.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.DataAccess
{
    [ExcludeFromCodeCoverage]
    [Collection("partner_profiles")]
    public class Partner : IMongoEntity<ObjectId>
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("partner_id")]
        public string PartnerId { get; set; }

        [BsonElement("partner_name")]
        public string? PartnerName { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("amount_of_locations")]
        public int AmountOfLocations { get; set; }

        [BsonElement("partner_type")]
        public string? PartnerType { get; set; }

        [BsonElement("service_agreement_accepted")]
        public bool ServiceAgreementAccepted { get; set; }

        [BsonElement("profiles")]
        public IEnumerable<string> UserProfiles { get; set; } = new List<string>();

        [BsonElement("is_active")]
        public bool IsActive { get; set; }

        [BsonElement("is_on_boarded")]
        public bool IsOnBoarded { get; set; }

        [BsonElement("subscription")]
        public Subscription Subscription { get; set; } = new Subscription();
    }
}
