using System;
using Akka.Actor;
using Akka.Cluster.Tools.Singleton;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaDotModule.Config
{
    public static class AkkaBoostrap
    {
        public static IServiceCollection AddAkka(this IServiceCollection services, ActorSystem actorSystem)
        {
            // Register ActorSystem
            services.AddSingleton<ActorSystem>((provider) => actorSystem);
            return services;
        }

        public static IActorRef BootstrapSingleton<T>(this ActorSystem system, string name, string role = null) where T : ActorBase
        {
            var props = ClusterSingletonManager.Props(
                singletonProps: Props.Create<T>(),
                settings: new ClusterSingletonManagerSettings(name, role, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(3)));

            return system.ActorOf(props, typeof(T).Name);
        }

        public static IActorRef BootstrapSingletonProxy(this ActorSystem system, string name, string role, string path, string proxyname)
        {
            var props = ClusterSingletonProxy.Props(
                singletonManagerPath: path,
                settings: new ClusterSingletonProxySettings(name, role, TimeSpan.FromSeconds(1), 100));

            return system.ActorOf(props, proxyname);
        }
    }
}
