using Actors.MissionPathPriority;
using CommandLine;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;

namespace TerminalUI.Verbs
{
    [Verb("spawn-mission", HelpText = "Crea una nuova missione")]
    internal class SpawnMission : IVerb
    {
        [Value(0, HelpText = "Coordinata X del punto di partenza", Required = true)]
        public double Xstart { get; set; }

        [Value(1, HelpText = "Coordinata Y del punto di partenza", Required = true)]
        public double Ystart { get; set; }

        [Value(2, HelpText = "Coordinata X del punto di arrivo", Required = true)]
        public double Xend { get; set; }

        [Value(3, HelpText = "Coordinata Y del punto di arrivo", Required = true)]
        public double Yend { get; set; }

        [Option('v', HelpText = "Velocità del drone", Default = 5.0f)]
        public float Speed { get; set; }

        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem su cui spawnare la missione", Required = true)]
        public int Port { get; set; }

        [Option('n', HelpText = "Nome della missione. Se non specificato viene generato automaticamente", Required = false)]
        public string? MissionName { get; set; }

        public Environment Run(Environment env)
        {
            if (!env.DroneDeliverySystemAPI.HasRepository())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Imposta prima un repository con spawn-repository o set-repository!");
                return env;
            }

            Host host = new Host(Host!, Port);
            if (!env.DroneDeliverySystemAPI.VerifyLocation(host))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore! nella locazione {host} non esiste un actor system attivo");
                return env;
            }

            var start = new MathNet.Spatial.Euclidean.Point2D(Xstart, Ystart);
            var end = new MathNet.Spatial.Euclidean.Point2D(Xend, Yend);
            if (start == end)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Il punto di partenza e di arrivo corrispondono");
                return env;
            }

            var missionPath = new MissionPath(start, end, Speed);

            // costruzione di un nome univoco per la missione
            MissionName = (MissionName is not null)
                ? MissionName 
                : missionPath.GetHashCode().ToString();
            var ID = $"{MissionName}-{Host}:{Port}";

            ObserverMissionAPI? missionAPI;

            try
            {
                missionAPI = env.DroneDeliverySystemAPI
                    .SpawnMission(host, missionPath, ID, 
                    ObserverMissionAPI.Factory(env.InterfaceActorSystem)) 
                    as ObserverMissionAPI;

                // ping
                _ = missionAPI!.GetCurrentStatus().Result;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Errore! La creazione della missione {MissionName} " +
                    $"sull'host {host} specificato è fallita per colpa di un'eccezione:\n{e}");
                return env;
            }

            var missionInfo = new MissionInfo(host, missionAPI);
            env.Missions.Add(ID, missionInfo);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Missione avviata!");
            Console.WriteLine($"Nome:\t{MissionName}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Usa il comando log {MissionName} -p{host.Port} [-h{host.HostName}] per " +
                $"leggere le notifiche ricevute fin'ora.");

            return env;
        }
    }
}
