namespace Tfs.WebHooks.Diagnostics
{
    using Serilog;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.ExceptionHandling;

    public class SerilogExceptionLogger : IExceptionLogger
    {
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            Log.Error(context.Exception, string.Empty);

            return Task.FromResult(true);
        }
    }
}