﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CXI.Common.MongoDb;
using GL.MSA.Core.NoSql.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClientWebAppService.PosProfile.Models
{
    /// <summary>
    /// MongoDB entity class, representing POS profile
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Collection("pos_profiles")]
    public class PosProfile : IMongoEntity<ObjectId>
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        [BsonElement("partner_id")]
        public string? PartnerId { get; set; }
        
        [BsonElement("is_historical_data_ingested")]
        public bool IsHistoricalDataIngested { get; set; }
            
        [BsonElement("historical_ingest_days_period")]
        public int HistoricalIngestDaysPeriod { get; set; }

        [BsonElement("pos_configuration")]
        public IEnumerable<PosCredentialsConfiguration>? PosConfiguration { get; set; }
    }

    /// <summary>
    /// Represents reference to credentials stored in KeyVault
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PosCredentialsConfiguration
    {
        [BsonElement("pos_type")]
        public string PosType { get; set; }
        
        [BsonElement("keyvault_reference")]
        public string KeyVaultReference { get; set; }

        [BsonElement("merchant_id")]
        public string MerchantId { get; set; }
    }
}