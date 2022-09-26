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

            _ = ActorSystemFactory.Create(out var _);

            return env;
        }
    }
}
