﻿using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.PartnerProfile.Core
{
    /// <summary>
    /// Common service constraints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CommonServiceConstraints
    {
        public static class AzureConfiguration
        {
            public const string KeyPrefix = "ClientWebAppServicePartnerProfile:";
        }
    }
}
