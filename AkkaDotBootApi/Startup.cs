using System;
using Akka.Actor;
using Akka.DI.Extensions.DependencyInjection;
using AkkaDotBootApi.Actor;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.Config;
using AkkaDotModule.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;

namespace AkkaDotBootApi
{
    public class Startup
    {
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

            // Akka ����
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var akkaConfig = AkkaLoad.Load(envName, Configuration);
            actorSystem = ActorSystem.Create("AkkaDotBootSystem", akkaConfig);
            var provider = services.BuildServiceProvider();
            actorSystem.UseServiceProvider(provider);
            services.AddAkka(actorSystem);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            lifetime.ApplicationStarted.Register(() =>
            {
                // ��� Work : �ʴ� �۾����� ����                
                int timeSec = 1;
                int elemntPerSec = 5;
                var throttleWork = AkkaLoad.RegisterActor("throttleWork", 
                    actorSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)), "throttleWork"));

                // ���� Work : ��꿡 ����Ǵ� Task�� ������ ó��
                var worker = AkkaLoad.RegisterActor("worker", actorSystem.ActorOf(Props.Create<WorkActor>(), "worker"));

                // ����� �۾��ڸ� ����
                throttleWork.Tell(new SetTarget(worker));

            });
        }
    }
}
