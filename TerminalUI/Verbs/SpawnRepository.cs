using Akka.Actor;
using CommandLine;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses.Utils;

namespace TerminalUI.Verbs
{
    [Verb("spawn-repository", HelpText = "Genera un repository locale o remoto")]
    internal class SpawnRepository : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem", Default = 0)]
        public int Port { get; set; }

        public Environment Run(Environment env)
        {
            ActorSystem? system;
            if (Host == "localhost" && !env.ActorSystems.ContainsKey(Port))
            {
                var configs = SystemConfigs.GenericConfig;
                configs.ActorName = "repository";

                system = ActorSystemFactory.Create(Port, out var port);
                if (system is not null)
                {
                    Port = port;
                }
                else
                    return env;
            }

            if (Host == "localhost" && env.ActorSystems.ContainsKey(Port))
            {
                system = env.ActorSystems[Port];
            }
            else
            {
                system = env.InterfaceActorSystem;
            }

            var repository = Spawn(system);
            if (repository is not null)
            {
                if (env.RepositoryAPI is null)
                {
                    env.RepositoryAPI = repository;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Repository impostato correttamente sul {new Host(Host!, Port)}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile creare il repository");
            }

            return env;
        }

        private IActorRef? Spawn(ActorSystem actorSystem)
        {
            var configs = SystemConfigs.GenericConfig;
            configs.ActorName = "repository";

            var repositoryActorRef =
                    new RepositoryProvider(actorSystem, configs)
                    .SpawnRemote(new Host(Host!, Port));

            if (repositoryActorRef is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Host Remoto non raggiungibile");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Error.WriteLine("Repository creato");
            }
            return repositoryActorRef;
        }
    }
}
