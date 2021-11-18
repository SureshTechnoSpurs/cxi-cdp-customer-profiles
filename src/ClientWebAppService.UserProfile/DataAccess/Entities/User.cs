using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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

        [BsonElement("role")]
        public string Role { get; set; }
    }
}
