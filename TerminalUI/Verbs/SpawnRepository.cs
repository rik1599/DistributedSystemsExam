using CommandLine;
using DroneSystemAPI;

namespace TerminalUI.Verbs
{
    [Verb("spawn-repository", HelpText = "Genera un repository su un host specificato")]
    internal class SpawnRepository : IVerb
    {
        [Option('p', HelpText = "Crea il repository sull'Actor System specificato nella porta.", Default = 0)]
        public int Port { get; set; }

        [Option('h', HelpText = "Indirizzo IP o nome host", Default = "localhost")]
        public string? Host { get; set; }

        public Environment Run(Environment env)
        {
            if (Host == "localhost" && !env.ActorSystems.ContainsKey(Port))
            {
                var configs = SystemConfigs.RepositoryConfig;
                configs.Port = Port;

                var system = ActorSystemFactory.CreateActorSystem(env, configs);
            }

            return env;
        }
    }
}
