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
            if (env.DroneDeliverySystemAPI.HasRepository())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore: allo stato attuale esiste già un repository: " +
                    $"{env.DroneDeliverySystemAPI.RepositoryAddress}.");
                return env;
            }
            
            if (Port == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile creare il repository senza impostare un host.");
                return env;
            }

            Host host = new Host(Host, Port);
            if (!env.DroneDeliverySystemAPI.VerifyLocation(host))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore! nella locazione {host} non esiste un actor system attivo");
                return env;
            }

            // var repository = Spawn(env);

            try
            {
                env.DroneDeliverySystemAPI.DeployRepository(host);
            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore durante il dispiegamento del repository\n{e}");
                return env;
            }

            // todo: remove
            env.RepositoryAPI = env.DroneDeliverySystemAPI.RepositoryAddress;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Repository creato correttamente: {env.DroneDeliverySystemAPI.RepositoryAddress}");

            /*
            if (repository is not null)
            {
                if (env.RepositoryAPI is null)
                {
                    env.RepositoryAPI = repository;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Repository creato impostato correttamente sul {new Host(Host!, Port)}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile creare il repository");
            } */

            return env;
        }

        private IActorRef? Spawn(Environment env)
        {
            var configs = SystemConfigs.GenericConfig;
            configs.ActorName = "repository";

            var repositoryActorRef =
                    new RepositoryProvider(env.InterfaceActorSystem, configs)
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
