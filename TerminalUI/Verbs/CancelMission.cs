using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("cancel-mission", HelpText = "Cancella una missione. Puoi cancellare solo missioni registrate")]
    internal class CancelMission : IVerb
    {
        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem a cui collegarsi", Required = true)]
        public int Port { get; set; }

        [Value(0, HelpText = "ID della missione", Required = true)]
        public int Mission { get; set; }

        public Environment Run(Environment env)
        {
            var ID = $"{Mission}-{Host!}:{Port}";
            if (!env.Missions.ContainsKey(ID))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Missione non registrata! Devi prima collegarti con connect-mission o spawn-mission");
                return env;
            }

            if (!env.Missions[ID].IsTerminated)
            {
                var missionAPI = env.Missions[ID].API;
                missionAPI.Cancel().Wait();
            }

            env.Missions.Remove(ID);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK: Missione cancellata");

            return env;
        }
    }
}
