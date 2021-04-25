using Akka.Actor;
using Akka.Streams;
using Akka.Streams.SignalR.AspNetCore;
using AkkaDotBootApi.Config;
using AkkaDotBootApi.Repositories;
using AkkaDotBootApi.SignalR;
using AkkaDotBootApi.Test;
using AkkaDotModule.Config;
using AkkaDotModule.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
using Akka.Monitoring.Datadog;
using StatsdClient;
using Akka.Monitoring;

namespace AkkaDotBootApi
{
    public class Startup
    {
        private string AppName = "AkkaDotBootApi";
        private string Company = "웹노리";
        private string CompanyUrl = "http://wiki.webnori.com/pages/viewpage.action?pageId=42467383";
        private string DocUrl = "http://wiki.webnori.com/display/AKKA";

        protected ActorSystem actorSystem;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton(Configuration.GetSection("AppSettings").Get<AppSettings>());// * AppSettings

            services.AddSingleton<ConsumerSystem>();
            services.AddSingleton<ProducerSystem>();

            services.AddDbContext<UserRepository>();

            // Akka 설정
            // 참고 :  https://getakka.net/articles/concepts/configuration.html
            // AKKASYSTEM : http://wiki.webnori.com/display/AKKA/AKKASYSTEM
            // Akka 셋팅
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var akkaConfig = AkkaLoad.Load(envName, Configuration);
            actorSystem = ActorSystem.Create("AkkaDotBootSystem", akkaConfig);

            services.AddAkka(actorSystem);

            //모니터링추가
            var statsdConfig = new StatsdConfig
            {
                StatsdServerName = "127.0.0.1"
            };
            ActorMonitoringExtension.RegisterMonitor(actorSystem, new ActorDatadogMonitor(statsdConfig));

            // Signal R 셋팅
            services
            .AddSingleton(new ConnectionSourceSettings(102400, OverflowStrategy.DropBuffer))
            .AddSignalRAkkaStream()            
            .AddSignalR();

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = AppName,
                    Description = $"{AppName} ASP.NET Core Web API",
                    TermsOfService = new Uri(CompanyUrl),
                    Contact = new OpenApiContact
                    {
                        Name = Company,
                        Email = "psmon@live.co.kr",
                        Url = new Uri(CompanyUrl),
                    },
                    License = new OpenApiLicense
                    {
                        Name = $"Document",
                        Url = new Uri(DocUrl),
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        }
                
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime lifetime)
        {            
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", AppName + "V1");
                c.RoutePrefix = "help";
            });

            if (!env.IsProduction())
            {
                using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    //로컬 개발모드에서는 Entity 자동생성
                    var userRepository = serviceScope.ServiceProvider.GetRequiredService<UserRepository>();
                    string currentConection = userRepository.Database.GetDbConnection().ConnectionString;
                    if (currentConection.Contains("localhost"))
                    {
                        //userRepository.Database.EnsureDeleted();
                        //userRepository.Database.EnsureCreated();
                    }

                    // ORM 마이그레이션은 다음을 참고합니다.
                    // https://docs.microsoft.com/ko-kr/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

                }
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();

                    endpoints.MapHub<EchoHub>("/echo");
                });

            lifetime.ApplicationStarted.Register(() =>
            {
                TestAkka.Start(app, actorSystem);
            });
        }
    }
}
