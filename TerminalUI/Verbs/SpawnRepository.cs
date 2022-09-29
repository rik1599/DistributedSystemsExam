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

        [Option('f', HelpText = "Forza la creazione del nuovo repository anche se c'è già uno.", Default = false)]
        public bool Force { get; set; }

        public Environment Run(Environment env)
        {
            if (Port == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile creare il repository senza impostare un host.");
                return env;
            }

            Host host = new Host(Host!, Port);

            if (!env.DroneDeliverySystemAPI.VerifyLocation(host))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore! nella locazione {host} non esiste un actor system attivo");
                return env;
            }


            if (env.DroneDeliverySystemAPI.HasRepository())
            {
                if (!Force)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"Errore: allo stato attuale esiste già un repository: " +
                        $"{env.DroneDeliverySystemAPI.RepositoryAddress}. Usa l'opzione -f per forzare l'operazione.");
                    return env;
                } else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Attenzione: il repository {env.DroneDeliverySystemAPI.RepositoryAddress} verrà sovrascritto.");
                    Console.WriteLine($"E' possibile re-impostarlo con il comando set-repository -pPorta [-hNomeHost] -f");
                }
            }
            
            try
            {
                env.DroneDeliverySystemAPI.DeployRepository(host);
            } catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore durante il dispiegamento del repository. Rilevata eccezione:\n{e}");
                return env;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Repository creato correttamente: {env.DroneDeliverySystemAPI.RepositoryAddress}");

            return env;
        }
    }
}
