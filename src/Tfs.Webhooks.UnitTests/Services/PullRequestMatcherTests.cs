namespace Tfs.WebHooks.UnitTests.Services
{
    using FluentAssertions;
    using Microsoft.AspNet.WebHooks.Payloads;
    using WebHooks.Services;
    using Xunit;

    public class PullRequestMatcherTests
    {
        private static readonly GitPullRequestUpdatedPayload _pullRequest = new GitPullRequestUpdatedPayload
        {
            Resource = new GitPullRequestUpdatedResource
            {
                Repository = new GitRepository
                {
                    Name = "repo-test"
                },
                Status = "Completed",
                TargetRefName = "master"
            }
        };

        [Theory]
        [InlineData("repo-.+", "master", "Completed")]
        [InlineData("REPO-.+", "master", "Completed")]
        [InlineData("repo-.+", "MaStEr", "Completed")]
        [InlineData("repo-.+", "master", "comPLEted")]
        public void WhenAllParametersMatch_ThenThePullRequestIsAMatch(
            string repositoryNamePattern,
            string targetBranchName,
            string status)
        {
            var sut = new PullRequestMatcher(
                repositoryNamePattern,
                targetBranchName,
                status);

            sut.IsMatch(_pullRequest).Should().BeTrue();
        }

        [Theory]
        [InlineData("underscore", "master", "Completed")]
        [InlineData("repo-other-test", "master", "Completed")]
        [InlineData("repo-.+", "dev", "Completed")]
        [InlineData("repo-.+", "master", "Updated")]
        public void WhenAtLeastOneParameterDoesntMatch_ThenThePullRequestIsNotAMatch(
            string repositoryNamePattern,
            string targetBranchName,
            string status)
        {
            var sut = new PullRequestMatcher(
                repositoryNamePattern,
                targetBranchName,
                status);

            sut.IsMatch(_pullRequest).Should().BeFalse();
        }
    }
}
