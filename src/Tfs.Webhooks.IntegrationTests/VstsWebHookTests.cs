namespace Tfs.WebHooks.IntegrationTests
{
    using FluentAssertions;
    using Microsoft.AspNet.WebHooks.Payloads;
    using Microsoft.Owin.Testing;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public class VstsWebHookTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public VstsWebHookTests()
        {
            _server = TestServer.Create<Startup>();
            _client = _server.HttpClient;
        }

        [Fact]
        public async Task WhenAMatchingPullRequestIsReceived_ThenTheResponseIs200()
        {
            var payload = GetPullRequestUpdatePayload("repo-test", "refs/heads/master", "completed");
            var response = await Post(payload);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task WhenANonMatchingPullRequestIsReceived_ThenTheResponseIs200()
        {
            var payload = GetPullRequestUpdatePayload("underscore", "refs/heads/dev", "not-completed");
            var response = await Post(payload);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task WhenTheCodeIsNotValid_ThenTheResponseIs400()
        {
            var payload = GetPullRequestUpdatePayload("repo-test", "refs/heads/master", "completed");
            var response = await Post(payload, code: null);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private Task<HttpResponseMessage> Post(GitPullRequestUpdatedPayload payload, string code = "83699ec7c1d794c0c780e49a5c72972590571fd8")
        {
            var uri = "/api/webhooks/incoming/vsts";
            if (!string.IsNullOrWhiteSpace(code))
            {
                uri += "?code=" + code;
            }

            return _client.PostAsJsonAsync(uri, payload);
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }

            if (_server != null)
            {
                _server.Dispose();
            }
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
