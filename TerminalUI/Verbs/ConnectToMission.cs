using CommandLine;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;

namespace TerminalUI.Verbs
{
    [Verb("connect-to-mission", HelpText = "Collegati a una missione remota per ricevere gli aggiornamenti")]
    internal class ConnectToMission : IVerb
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
            if (env.Missions.ContainsKey(ID))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Missione già osservata!");
                return env;
            }

            var configs = SystemConfigs.GenericConfig;
            var host = new Host(Host!, Port);

            if (new MissionProvider(env.InterfaceActorSystem, configs)
                .TryConnectToExistent(host, ID) is not ObserverMissionAPI missionAPI)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile collegarsi alla missione");
            }
            else
            {
                env.Missions.Add(ID, new MissionInfo(host, missionAPI));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Missione aggiunta a quelle osservate");
            }
            return env;
        }
    }
}
