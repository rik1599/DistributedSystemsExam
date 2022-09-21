using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Remote;
using CommandLine;
using DroneSystemAPI;

namespace TerminalUI.Verbs
{
    [Verb("create-actor-system", HelpText = "Genera un ActorSystem su questo Host con porta specificata")]
    internal class CreateActorSystem : IVerb
    {
        [Option('p', HelpText = "Porta su cui generare l'ActorSystem", Default = 0)]
        public int Port { get; set; }
        public Environment Run(Environment env)
        {
            var config = SystemConfigs.DroneConfig;
            config.Port = Port;

            _ = ActorSystemFactory.CreateActorSystem(env, config);
            
            return env;
        }
    }
}
