namespace Tfs.WebHooks.Diagnostics
{
    using Microsoft.AspNet.WebHooks.Diagnostics;
    using System;
    using System.Web.Http.Tracing;
    using SerilogLog = Serilog.Log;

    public class WebHooksSerilogLogger : ILogger
    {
        public void Log(TraceLevel level, string message, Exception ex)
        {
            switch (level)
            {
                case TraceLevel.Debug:
                    SerilogLog.Debug(message);
                    break;

                case TraceLevel.Info:
                    SerilogLog.Information(message);
                    break;

                case TraceLevel.Warn:
                    SerilogLog.Warning(message);
                    break;

                case TraceLevel.Error:
                    SerilogLog.Error(ex, message);
                    break;

                case TraceLevel.Fatal:
                    SerilogLog.Fatal(ex, message);
                    break;
            }
        }
    }
}