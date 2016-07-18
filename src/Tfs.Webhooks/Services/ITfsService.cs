namespace Tfs.WebHooks.Services
{
    using System.Threading.Tasks;

    public interface ITfsService
    {
        Task<bool> ArePendingBuilds(string projectName, string buildDefinitionName);

        Task QueueNewBuild(string projectName, string buildDefinitionName);
    }
}