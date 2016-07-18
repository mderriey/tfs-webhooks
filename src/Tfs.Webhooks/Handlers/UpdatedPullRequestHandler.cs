namespace Tfs.WebHooks.Handlers
{
    using Tfs.WebHooks.Services;
    using Microsoft.AspNet.WebHooks.Payloads;
    using Serilog;
    using Services;
    using System.Threading.Tasks;

    public class UpdatedPullRequestHandler
    {
        private readonly PullRequestMatcher _pullRequestMatcher;
        private readonly ITfsService _tfsService;
        private readonly string _projectName;
        private readonly string _buildDefinitionName;

        public UpdatedPullRequestHandler(
            PullRequestMatcher pullRequestMatcher,
            ITfsService tfsService,
            string projectName,
            string buildDefinitionName)
        {
            _pullRequestMatcher = pullRequestMatcher;
            _tfsService = tfsService;
            _projectName = projectName;
            _buildDefinitionName = buildDefinitionName;
        }

        public async Task Handle(GitPullRequestUpdatedPayload pullRequest)
        {
            if (_pullRequestMatcher.IsMatch(pullRequest))
            {
                Log.Information("Pull request matches, checking for pending builds for {BuildDefinitionName} on project {ProjectName}", _buildDefinitionName, _projectName);
                if (await _tfsService.ArePendingBuilds(_projectName, _buildDefinitionName))
                {
                    Log.Information("Found pending build(s), not queuing another one");
                }
                else
                {
                    Log.Information("No build(s) pending, queuing another one");
                    await _tfsService.QueueNewBuild(_projectName, _buildDefinitionName);
                    Log.Information("New build successfully queued");
                }
            }
        }
    }
}