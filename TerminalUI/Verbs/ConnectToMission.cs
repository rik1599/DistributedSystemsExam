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

        [Value(0, HelpText = "Nome della missione", Required = true)]
        public string? MissionName { get; set; }

        public Environment Run(Environment env)
        {
            var ID = $"{MissionName}-{Host!}:{Port}";
            if (env.Missions.ContainsKey(ID))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Missione già osservata!");
                return env;
            }

            var host = new Host(Host!, Port);

            ObserverMissionAPI? missionAPI;

            try
            {
                missionAPI = env.DroneDeliverySystemAPI
                    .ConnectToMission(host, ID,
                    ObserverMissionAPI.Factory(env.InterfaceActorSystem)) 
                    as ObserverMissionAPI;

                if (missionAPI is null)
                    throw new Exception("L''istanza ricevuta è nulla");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore! Impossibile collegarsi alla missione {MissionName} " +
                    $"su {host}. E' possibile che la missione sia già terminata (oppure mai esistita)." +
                    $"\nEccezione rilevata:\n{e}");
                return env;
            }

            env.Missions.Add(ID, new MissionInfo(host, missionAPI!));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"La missione {MissionName} (eseguita su {host})" +
                $"è stata aggiunta a quelle osservate.");
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Usa il comando log {MissionName} -p{host.Port} [-h{host.HostName}] per " +
                $"leggere le notifiche ricevute fin'ora.");

            return env;
        }
    }
}
