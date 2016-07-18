namespace Tfs.WebHooks.UnitTests.Handlers
{
    using Microsoft.AspNet.WebHooks.Payloads;
    using NSubstitute;
    using System.Threading.Tasks;
    using WebHooks.Handlers;
    using WebHooks.Services;
    using Xunit;

    public class UpdatedPullRequestHandlerTests
    {
        private const string ProjectName = "ProjectName";
        private const string BuildDefinitionName = "Project-BuildDefinitionName";

        private readonly UpdatedPullRequestHandler _sut;
        private readonly PullRequestMatcher _matcher;
        private readonly ITfsService _tfsService;

        public UpdatedPullRequestHandlerTests()
        {
            _tfsService = Substitute.For<ITfsService>();
            _matcher = new PullRequestMatcher("repo-.+", "master", "completed");

            _sut = new UpdatedPullRequestHandler(
                _matcher,
                _tfsService,
                ProjectName,
                BuildDefinitionName);
        }

        [Fact]
        public async Task WhenThePullRequestIsAMatch_AndNoBuildIsPending_ThenABuildIsQueued()
        {
            _tfsService
                .ArePendingBuilds(ProjectName, BuildDefinitionName)
                .Returns(false);

            var pullRequest = GetPullRequestUpdatePayload(
                "repo-test",
                "master",
                "completed");

            await _sut.Handle(pullRequest);

            await _tfsService
                .Received()
                .QueueNewBuild(ProjectName, BuildDefinitionName);
        }

        [Fact]
        public async Task WhenThePullRequestIsAMatch_AndOneOrMoreBuildsArePending_ThenABuildIsNotQueued()
        {
            _tfsService
                .ArePendingBuilds(ProjectName, BuildDefinitionName)
                .Returns(true);

            var pullRequest = GetPullRequestUpdatePayload(
                "repo-test",
                "master",
                "completed");

            await _sut.Handle(pullRequest);

            await _tfsService
                .DidNotReceive()
                .QueueNewBuild(ProjectName, BuildDefinitionName);
        }

        [Fact]
        public async Task WhenThePullRequestIsNotAMatch_ThenABuildIsNotQueued()
        {
            var pullRequest = GetPullRequestUpdatePayload(
                "underscore",
                "dev",
                "updated");

            await _sut.Handle(pullRequest);

            await _tfsService
                .DidNotReceive()
                .QueueNewBuild(ProjectName, BuildDefinitionName);
        }

        private static GitPullRequestUpdatedPayload GetPullRequestUpdatePayload(
            string repositoryName,
            string targetBranch,
            string status)
        {
            return new GitPullRequestUpdatedPayload
            {
                Resource = new GitPullRequestUpdatedResource
                {
                    Repository = new GitRepository
                    {
                        Name = repositoryName
                    },
                    Status = status,
                    TargetRefName = targetBranch
                }
            };
        }
    }
}
