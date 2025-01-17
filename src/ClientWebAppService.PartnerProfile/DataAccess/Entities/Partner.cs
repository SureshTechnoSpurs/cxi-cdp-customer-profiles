﻿using CXI.Common.MongoDb;
using CXI.Contracts.PartnerProfile.Models;
using GL.MSA.Core.NoSql.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
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

        [BsonElement("service_agreement_version")]
        public string? ServiceAgreementVersion { get; set; }

        [BsonElement("service_agreement_accepted_date")]
        public DateTime? ServiceAgreementAcceptedDate { get; set; }

        [BsonElement("created_on")]
        public DateTime CreatedOn { get; set; }

        [BsonElement("synthetic_generate_flag")]
        public bool SyntheticGenerateFlag { get; set; }

        [BsonElement("ui_enable_flag")]
        public bool UiEnableFlag { get; set; }

        [BsonElement("demog_predict_flag")]
        public bool DemogPredictFlag { get; set; }

        [BsonElement("tutorial_enable_flag")]
        public bool TutorialEnableFlag { get; set; }

        [BsonElement("overview_dashboard_flag")]
        public bool OverviewDashboardFlag { get; set; }

        [BsonElement("identity_phone_flag")]
        public bool IdentityPhoneFlag { get; set; }

        [BsonElement("identity_email_flag")]
        public bool IdentityEmailFlag { get; set; }

        [BsonElement("identity_idfa_flag")]
        public bool IdentityIOSFlag { get; set; }

        [BsonElement("identity_aaid_flag")]
        public bool IdentityAndroidFlag { get; set; }
    }
}
