using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Travel.WebAPI.Utils
{
    public static class Log4netHelper
    {
        public static IWebHostBuilder ConfigureLog4net(this IWebHostBuilder webHost)
        {
            var log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));

            var loggerRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                typeof(log4net.Repository.Hierarchy.Hierarchy));

            log4net.Config.XmlConfigurator.Configure(loggerRepository, log4netConfig["log4net"]);

            webHost.ConfigureLogging(logging => {
                logging.ClearProviders();
                logging.AddLog4Net();
            });

            return webHost;
        }
    }
}