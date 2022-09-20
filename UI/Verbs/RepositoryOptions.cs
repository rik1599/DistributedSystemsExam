using Akka.Actor;
using CommandLine;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses.Repository;

namespace UI.Verbs
{
    [Verb("repository", HelpText = "start terminal as a repository")]
    internal class RepositoryOptions
    {
        public static int Run(RepositoryOptions options)
        {
            var configs = SystemConfigs.RepositoryConfig;
            using var system = ActorSystem.Create(configs.SystemName, configs.HoconConfig);
            _ = new RepositoryProvider(system, configs).SpawnHere();
            Console.ReadKey();
            return 0;
        }
    }
}
