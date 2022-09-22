using CommandLine;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI;

namespace TerminalUI.Verbs
{
    [Verb("set-repository", HelpText = "Imposta il repository per le missioni spawnate da questo terminale")]
    internal class SetRepository : IVerb
    {
        [Value(0, Required = true, HelpText = "Porta dell'ActorSystem con il repository")]
        public int Port { get; set; }

        public Environment Run(Environment env)
        {
            if (env.ActorSystems.ContainsKey(Port))
            {
                var configs = SystemConfigs.GenericConfig;
                configs.ActorName = "repository";

                var repositoryAPI = new RepositoryProvider(env.ActorSystems[Port], configs)
                    .TryConnectToExistent(new Host("localhost", Port));
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
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore: porta non utilizzata! Inizializzare prima un repository con spawn-repository");
            }

            return env;
        }
    }
}
