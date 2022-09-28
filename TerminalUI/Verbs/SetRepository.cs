using CommandLine;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI;

namespace TerminalUI.Verbs
{
    [Verb("set-repository", HelpText = "Imposta il repository per le missioni spawnate da questo terminale")]
    internal class SetRepository : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
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
                Console.Error.WriteLine("Errore! Impossibile impostare un repository senza impostare un host.");
                return env;
            }

            Host host = new Host(Host!, Port);

            try
            {
                env.DroneDeliverySystemAPI.SetRepository(host);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore durante il collegamento con il repository\n{e}");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Repository impostato correttamente: {env.DroneDeliverySystemAPI.RepositoryAddress}");


            return env;
            /*
            var configs = SystemConfigs.GenericConfig;
            configs.ActorName = "repository";

            var repositoryAPI = new RepositoryProvider(env.InterfaceActorSystem, configs)
                .TryConnectToExistent(new Host(Host!, Port));

            if (repositoryAPI is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore: repository non trovato!");
            }
            else
            {
                env.RepositoryAPI = repositoryAPI;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Repository impostato correttamente sulla porta {Port}");
            }

            return env;*/
        }
    }
}
