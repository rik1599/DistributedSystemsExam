using CommandLine;
using DroneSystemAPI.APIClasses.Repository;
using DroneSystemAPI.APIClasses;

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
                var repositoryAPI = new RepositoryProvider(env.ActorSystems[Port])
                    .TryConnectToExistent(new Host("localhost", Port));
                if (repositoryAPI is null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine("Errore: impossibile collegarsi al repository!");
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
                Console.Error.WriteLine("Errore: porta non utilizzata o non assegnata a un repository!");
            }
            Console.ResetColor();
            return env;
        }
    }
}
