using ClientWebAppService.PartnerProfile.Business.Utils;
using Xunit;

namespace ClientWebAppService.PartnerProfile.Business.Tests
{
    public class PartnerProfileUtilsTests
    {
        private const string PartnerIdPrefix = "cxi-usa-";

        [Fact]
        public void GetPartnerIdByName_IncorrectPartnerNameProvided_FormatedPartnerIdReturned()
        {
            string partnerName = "  Partner     Name    1";
            string expectedPartnerName = $"{PartnerIdPrefix}partnername1";

            string resultingPartnerName = PartnerProfileUtils.GetPartnerIdByName(partnerName);

            Assert.StartsWith(expectedPartnerName, resultingPartnerName);
        }
    }
}
