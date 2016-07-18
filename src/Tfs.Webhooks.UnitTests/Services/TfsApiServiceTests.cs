namespace Tfs.WebHooks.UnitTests.Services
{
    using FluentAssertions;
    using System;
    using System.Threading.Tasks;
    using WebHooks.Services;
    using Xunit;

    public class TfsApiServiceTests
    {
        private const string TeamProjectCollectionUrl = "https://tfs/DefaultCollection";
        private const string ProjectName = "ProjectName";
        private const string BuildDefinitionName = "Project-BuildDefinitionName";

        private readonly TfsApiService _sut;

        public TfsApiServiceTests()
        {
            _sut = new TfsApiService(TeamProjectCollectionUrl);
        }

        [Fact]
        public void WhenCallingAreBuildsPending_NoExceptionIsThrown()
        {
            Func<Task> action = async () => await _sut.ArePendingBuilds(ProjectName, BuildDefinitionName);
            action.ShouldNotThrow();
        }

        [Fact]
        public void WhenCallingQueueNewBuild_NoExceptionIsThrown()
        {
            Func<Task> action = async () => await _sut.QueueNewBuild(ProjectName, BuildDefinitionName);
            action.ShouldNotThrow();
        }
    }
}
