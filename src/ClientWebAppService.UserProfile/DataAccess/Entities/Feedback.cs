using CXI.Contracts.UserProfile.Models;
using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;
using System;

namespace ClientWebAppService.UserProfile.DataAccess
{
    [ExcludeFromCodeCoverage]
    [Collection("partner_feedback")]
    public class Feedback : IMongoEntity<ObjectId>
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("partner_id")]
        public string PartnerId { get; set; }

        [BsonElement("user_email")]
        public string Email { get; set; }

        [BsonElement("partner_name")]
        public string PartnerName { get; set; }

        [BsonElement("subject")]
        public string Subject { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("created_on")]
        public DateTime CreatedOn { get; set; }
    }
}
