namespace Tfs.WebHooks.MessageHandlers
{
    using Serilog.Context;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class SerilogRequestIdHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("RequestId", request.GetCorrelationId()))
            {
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}