using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;

namespace AkkaDotBootApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var nlogEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (nlogEnvironment == "Production")
            {
                LogManager.LoadConfiguration("NLog.config");
            }
            else
            {
                LogManager.LoadConfiguration(nlogEnvironment == "" ? "NLog.config" : $"NLog.{nlogEnvironment}.config");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
.ConfigureWebHostDefaults(webBuilder =>
{
    var config = GetServerUrlsFromCommandLine(args);
    var hostUrl = config.GetValue<string>("server.urls");

    webBuilder.ConfigureKestrel(serverOptions =>
    {
        // Set properties and call methods on options
        //TODO : Index 서비스에서만 타임아웃 5분설정하기
        serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
    })
    .UseUrls(hostUrl)
    .UseStartup<Startup>()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
    })
    .UseNLog();

});
        }

        public static IConfigurationRoot GetServerUrlsFromCommandLine(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var serverPort = Environment.GetEnvironmentVariable("Port") ?? "5000";
            var listenIp = Environment.GetEnvironmentVariable("server.urls") ?? "http://0.0.0.0";
            var serverurls = string.Format("{0}:{1}", listenIp, serverPort);

            Console.WriteLine($"================ SERVER Start {serverurls}");

            var configDictionary = new Dictionary<string, string>
            {
                {"server.urls", serverurls}
            };

            return new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddInMemoryCollection(configDictionary)
                .Build();
        }
    }
}
