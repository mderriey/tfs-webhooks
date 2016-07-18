namespace Tfs.WebHooks.Services
{
    using Microsoft.TeamFoundation.Build.WebApi;
    using Microsoft.VisualStudio.Services.Client;
    using Microsoft.VisualStudio.Services.Common;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TfsApiService : ITfsService
    {
        private static readonly Dictionary<string, int> _buildDefinitionIdCache = new Dictionary<string, int>();
        private readonly VssConnection _connection;

        public TfsApiService(string teamProjectCollectionUri)
        {
            _connection = new VssConnection(
                new Uri(teamProjectCollectionUri),
                new VssCredentials(true));
        }

        public async Task<bool> ArePendingBuilds(string projectName, string buildDefinitionName)
        {
            var buildClient = await _connection.GetClientAsync<BuildHttpClient>();
            var buildDefinitionId = await GetBuildDefinitionId(projectName, buildDefinitionName);

            var pendingBuilds = await buildClient.GetBuildsAsync(
                project: projectName,
                definitions: new[] { buildDefinitionId },
                statusFilter: BuildStatus.NotStarted | BuildStatus.Postponed);

            return pendingBuilds.Count != 0;
        }

        public async Task QueueNewBuild(string projectName, string buildDefinitionName)
        {
            var buildClient = await _connection.GetClientAsync<BuildHttpClient>();
            var buildDefinitionId = await GetBuildDefinitionId(projectName, buildDefinitionName);

            var buildDefinition = await buildClient.GetDefinitionAsync(projectName, buildDefinitionId);

            var newBuild = new Build
            {
                Definition = new DefinitionReference
                {
                    Id = buildDefinition.Id
                },
                Project = buildDefinition.Project
            };

            await buildClient.QueueBuildAsync(newBuild);
        }

        private async Task<int> GetBuildDefinitionId(string projectName, string buildDefinitionName)
        {
            var cacheKey = "{0}:{1}".FormatWith(projectName, buildDefinitionName);
            if (_buildDefinitionIdCache.ContainsKey(cacheKey))
            {
                Log.Information("Found build definition id for build {BuildDefinitionName} of project {ProjectName} in the cache", buildDefinitionName, projectName);
                return _buildDefinitionIdCache[cacheKey];
            }

            Log.Information("Couldn't find the build definition id for build {BuildDefinitionName} of project {ProjectName} in the cache, calling the API to get it",
                buildDefinitionName,
                projectName);

            var buildClient = await _connection.GetClientAsync<BuildHttpClient>();
            var buildDefinitions = await buildClient.GetDefinitionsAsync(project: projectName, name: buildDefinitionName);

            if (buildDefinitions.Count == 0)
            {
                throw new Exception("Could not find a build named {0} in project {1}".FormatWith(buildDefinitionName, projectName));
            }

            var buildDefinitionId = buildDefinitions[0].Id;

            _buildDefinitionIdCache.Add(cacheKey, buildDefinitionId);
            return buildDefinitionId;
        }
    }
}