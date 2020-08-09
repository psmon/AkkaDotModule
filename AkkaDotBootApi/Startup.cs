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

            // Akka 셋팅
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
                // 밸브 Work : 초당 작업량을 조절                
                int timeSec = 1;
                int elemntPerSec = 5;
                var throttleWork = AkkaLoad.RegisterActor("throttleWork", 
                    actorSystem.ActorOf(Props.Create(() => new ThrottleWork(elemntPerSec, timeSec)), "throttleWork"));

                // 실제 Work : 밸브에 방출되는 Task를 개별로 처리
                var worker = AkkaLoad.RegisterActor("worker", actorSystem.ActorOf(Props.Create<WorkActor>(), "worker"));

                // 배브의 작업자를 지정
                throttleWork.Tell(new SetTarget(worker));

            });
        }
    }
}
