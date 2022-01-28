using CXI.Contracts.UserProfile.Models;
using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.UserProfile.DataAccess
{
    [ExcludeFromCodeCoverage]
    [Collection("user_profiles")]
    public class User : IMongoEntity<ObjectId>
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("partner_id")]
        public string PartnerId { get; set; }

        [BsonElement("user_email")]
        public string Email { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("role")]
        public UserRole Role { get; set; }

        [BsonElement("invitation_accepted")]
        public bool? InvitationAccepted { get; set; }
    }
}
