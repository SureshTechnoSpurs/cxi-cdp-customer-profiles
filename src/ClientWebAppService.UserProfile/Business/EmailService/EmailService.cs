using ClientWebAppService.UserProfile.Core;
using CXI.Common.MessageBrokers.Producers;
using CXI.Common.MessageBrokers.Utilities;
using CXI.MessageBroker.Messages.Emailing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CXI.Common.Utilities;
using Microsoft.Extensions.Logging;

namespace ClientWebAppService.UserProfile.Business
{
    ///<inheritdoc cref="IEmailService"/>
    public class EmailService : IEmailService
    {
        private readonly IProducer _producer;
        private readonly AdB2CInvitationOptions _b2cIdentityOptions;
        private const string EmailSubject = "You have been invited as an associate";
        private const string B2CSignUpUrl = "{0}/{1}/{2}/oauth2/v2.0/authorize?client_id={3}&nonce={4}&redirect_uri={5}&scope=openid&response_type=id_token";
        private const string EmailTemplatePath = "EmailTemplates/InvitationTemplate.html";
        private readonly ILogger<EmailService> _logger;

        public EmailService(IProducer producer,
                            IOptions<AdB2CInvitationOptions> b2cIDentityOptions,
                            ILogger<EmailService> logger)
        {
            _producer = producer;
            _b2cIdentityOptions = b2cIDentityOptions.Value;
            _logger = logger;
        }

        ///<inheritdoc/>
        public async Task SendInvitationMessageToAssociateAsync(string email)
        {
            _logger.LogInformation($"Sending Inviatation for Email: {email}");

            var inviteUrl = BuildInvitationUrl(email);
            _logger.LogInformation($"Inviatation Url: {inviteUrl}");

            string htmlTemplate = File.ReadAllText(EmailTemplatePath);
            _logger.LogInformation($"Html Email Template : {htmlTemplate}");

            var emailMessage = new EmailDataMessage
            {
                To = email,
                Subject = EmailSubject,
                HtmlContent = string.Format(htmlTemplate, inviteUrl),
                CorrelationId = GetCorrelationIdForMessage()
            };

            await _producer.SendMessages(new string[] { emailMessage.Serialize() });
        }

        private string BuildInvitationUrl(string email)
        {
            string token = BuildSymmetricJwtToken(email);
            string nonce = Guid.NewGuid().ToString("n");

            return string.Format(B2CSignUpUrl,
                    _b2cIdentityOptions.Instance,
                    _b2cIdentityOptions.Domain,
                    _b2cIdentityOptions.SignUpSignInPolicyId,
                    _b2cIdentityOptions.InvitationClientId,
                    nonce,
                    Uri.EscapeDataString(_b2cIdentityOptions.RedirectUrl))
                + "&id_token_hint=" + token;
        }

        private string BuildSymmetricJwtToken(string email)
        {
            IList<System.Security.Claims.Claim> claims = new List<System.Security.Claims.Claim>();
            claims.Add(new System.Security.Claims.Claim("email", email, System.Security.Claims.ClaimValueTypes.String));

            var securityKey = Encoding.UTF8.GetBytes(_b2cIdentityOptions.TokenSecurityKey);

            var signingKey = new SymmetricSecurityKey(securityKey);
            SigningCredentials signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                    _b2cIdentityOptions.TokenIssuer,
                    _b2cIdentityOptions.TokenAudience,
                    claims,
                    DateTime.Now,
                    DateTime.Now.AddDays(7),
                    signingCredentials);

            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();

            return jwtHandler.WriteToken(token);
        }

        private Guid GetCorrelationIdForMessage()
        {
            var correaltionid = TelemetryHelper.GetCorrelationIdForCurrentActivity();
            return Guid.TryParse(correaltionid, out var messageCorrelationId) ? messageCorrelationId : Guid.NewGuid();      
        }
    }
}
