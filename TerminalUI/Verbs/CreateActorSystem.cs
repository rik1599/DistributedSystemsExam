using CommandLine;
using DroneSystemAPI;
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
            var config = SystemConfigs.GenericConfig;
            config.Port = Port;

            try
            {
                _ = ActorSystemFactory.Create(out var port);
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
