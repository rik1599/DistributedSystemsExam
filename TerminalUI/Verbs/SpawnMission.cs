using Actors.MissionPathPriority;
using CommandLine;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission;
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

            var start = new MathNet.Spatial.Euclidean.Point2D(Xstart, Ystart);
            var end = new MathNet.Spatial.Euclidean.Point2D(Xend, Yend);
            if (start == end)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Il punto di partenza e di arrivo corrispondono");
                return env;
            }

            var mission = new MissionPath(start, end, Speed);
            var config = SystemConfigs.GenericConfig;
            var host = new Host(Host!, Port);
            MissionName = MissionName is not null ? MissionName : mission.GetHashCode().ToString();
            var ID = $"{MissionName}-{Host}:{Port}";
            var missionAPI = new MissionSpawner(
                env.InterfaceActorSystem,
                env.DroneDeliverySystemAPI.RepositoryAddress!,
                ObserverMissionAPI.Factory(env.InterfaceActorSystem),
                config).SpawnRemote(host, mission, ID) as ObserverMissionAPI;

            try
            {
                _ = missionAPI!.GetCurrentStatus().Result;
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errore! Impossibile creare la missione sull'host specificato");
                return env;
            }

            var missionInfo = new MissionInfo(host, missionAPI);
            env.Missions.Add(ID, missionInfo);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(MissionName);

            return env;
        }
    }
}
