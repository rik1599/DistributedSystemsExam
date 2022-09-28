using CommandLine;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Utils;

namespace TerminalUI.Verbs
{
    [Verb("create-actor-system", HelpText = "Genera un ActorSystem locale con porta specificata")]
    internal class CreateActorSystem : IVerb
    {
        [Option('p', HelpText = "Porta su cui generare l'ActorSystem", Default = 0)]
        public int Port { get; set; }
        public Environment Run(Environment env)
        {
            try
            {
                _ = ActorSystemFactory.Create(

                    // dettagli della locazione
                    new DeployPointDetails(
                        new Host("localhost", Port), 
                        Config2.Default().SystemName
                        ), 

                    // porta reale dove viene creato il sistema
                    // (in caso che io abbia passato 0 come input)
                    out var port);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Actor system creato alla porta {port}");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERRORE: {e.Message}");
            }

            return env;
        }
    }
}
