using GL.MSA.Core.NoSql;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ClientWebAppService.TMI.DataAccess.Core
{
    /// <summary>
    /// Custom for TMI service MongoClientProvider that subscribe for mongo events,
    /// and write additional dependency telemetry.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PartnerProfileMongoClientProvider : IMongoClientProvider
    {
        private const string MongoDbDepencencyName = "MongoDB.PartnerProfile";

        private readonly TelemetryClient _telemetryClient;
        private MongoUrl _mongoUrl;

        public PartnerProfileMongoClientProvider(TelemetryClient telemetryClient) =>
            _telemetryClient = telemetryClient;

        public MongoClient CreateClient(string connectionString)
        {
            _mongoUrl = new MongoUrl(connectionString);
            var mongoClientSettings = MongoClientSettings.FromUrl(_mongoUrl);

            mongoClientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(OnQueryStarted);
                cb.Subscribe<CommandSucceededEvent>(OnQuerySuccessed);
                cb.Subscribe<CommandFailedEvent>(OnQueryFailed);
            };

            var mongoClient = new MongoClient(mongoClientSettings);
            return mongoClient;
        }

        private void OnQueryStarted(CommandStartedEvent e)
        {
            var telemetry =
                       new DependencyTelemetry(dependencyTypeName: "Storage command started",
                                               target: _mongoUrl.Server.ToString(),
                                               dependencyName: MongoDbDepencencyName,
                                               e.Command.ToJson());

            _telemetryClient.TrackDependency(telemetry);
        }

        private void OnQuerySuccessed(CommandSucceededEvent e)
        {
            var telemetry =
                new DependencyTelemetry(dependencyTypeName: "Storage command successed",
                                        target: _mongoUrl.Server.ToString(),
                                        dependencyName: MongoDbDepencencyName,
                                        data: e.Reply.ToJson(),
                                        startTime: DateTime.UtcNow - e.Duration,
                                        duration: e.Duration,
                                        resultCode: e.CommandName,
                                        success: true);

            _telemetryClient.TrackDependency(telemetry);
        }

        private void OnQueryFailed(CommandFailedEvent e)
        {
            var telemetry =
                new DependencyTelemetry(dependencyTypeName: "Storage command failed",
                                        target: _mongoUrl.Server.ToString(),
                                        dependencyName: MongoDbDepencencyName,
                                        data: e.Failure.ToJson(),
                                        startTime: DateTime.UtcNow - e.Duration,
                                        duration: e.Duration,
                                        resultCode: e.CommandName,
                                        success: false);

            _telemetryClient.TrackDependency(telemetry);
        }
    }
}
