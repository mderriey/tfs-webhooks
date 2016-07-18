namespace Tfs.WebHooks.WebHookHandlers
{
    using Handlers;
    using Microsoft.AspNet.WebHooks;
    using Microsoft.AspNet.WebHooks.Payloads;
    using System.Threading.Tasks;

    public class VstsWebHookHandler : VstsWebHookHandlerBase
    {
        private readonly UpdatedPullRequestHandler _updatedPullRequestHandler;

        public VstsWebHookHandler(UpdatedPullRequestHandler updatedPullRequestHandler)
        {
            _updatedPullRequestHandler = updatedPullRequestHandler;
        }

        public override Task ExecuteAsync(WebHookHandlerContext context, GitPullRequestUpdatedPayload payload)
        {
            return _updatedPullRequestHandler.Handle(payload);
        }
    }
}