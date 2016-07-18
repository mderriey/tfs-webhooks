namespace Tfs.WebHooks
{
    using Autofac;
    using Autofac.Integration.WebApi;
    using Diagnostics;
    using Handlers;
    using MessageHandlers;
    using Microsoft.AspNet.WebHooks;
    using Owin;
    using Serilog;
    using Services;
    using System.Configuration;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;
    using WebHookHandlers;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureSerilog();

            var container = GetContainer();

            var configuration = new HttpConfiguration();
            configuration.MapHttpAttributeRoutes();
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            configuration.MessageHandlers.Add(new SerilogRequestIdHandler());
            configuration.InitializeReceiveVstsWebHooks();

            app.UseWebApi(configuration);
        }

        private static void ConfigureSerilog()
        {
            var environment = ConfigurationManager.AppSettings["Environment"];
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{RequestId}] [{Level:u3}] {Message}{NewLine}{Exception}";
            var filePath = ConfigurationManager.AppSettings["LogFilePath"];
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(filePath, outputTemplate: outputTemplate);

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static IContainer GetContainer()
        {
            var builder = new ContainerBuilder();

            // WebHooks handlers
            builder
                .RegisterType<VstsWebHookHandler>()
                .As<IWebHookHandler>();

            // Handlers
            builder
                .Register(x => new UpdatedPullRequestHandler(
                    x.Resolve<PullRequestMatcher>(),
                    x.Resolve<ITfsService>(),
                    ConfigurationManager.AppSettings["TargetTeamProjectName"],
                    ConfigurationManager.AppSettings["TargetBuildDefinitionName"]))
                .AsSelf()
                .SingleInstance();

            // Pull requests matcher
            builder
                .Register(x => new PullRequestMatcher(
                    ConfigurationManager.AppSettings["PullRequestRepositoryPattern"],
                    ConfigurationManager.AppSettings["PullRequestTargetBranchName"],
                    ConfigurationManager.AppSettings["PullRequestStatus"]))
                .AsSelf()
                .SingleInstance();

            // TFS service
            builder
                .Register(x => new TfsApiService(ConfigurationManager.AppSettings["TeamProjectCollectionUri"]))
                .As<ITfsService>()
                .SingleInstance();

            // Loggers
            builder
                .RegisterType<WebHooksSerilogLogger>()
                .As<Microsoft.AspNet.WebHooks.Diagnostics.ILogger>()
                .SingleInstance();

            builder
                .RegisterType<SerilogExceptionLogger>()
                .As<IExceptionLogger>()
                .SingleInstance();

            return builder.Build();
        }
    }
}