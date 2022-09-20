using Actors.MissionPathPriority;
using Akka.Actor;
using CommandLine;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses.Mission.SimpleMissionAPI;
using DroneSystemAPI.APIClasses.Repository;
using MathNet.Spatial.Euclidean;

namespace UI.Verbs
{
    [Verb("drone", HelpText = "Start terminal as a drone with given coordinates")]
    internal class DroneOptions
    {
        // Coordinate
        [Option(Required = true)]
        public double Xstart { get; set; }

        [Option(Required = true)]
        public double Ystart { get; set; }

        [Option(Required = true)]
        public double Xend { get; set; }
        
        [Option(Required = true)]
        public double Yend { get; set; }

        [Option(Default = 5.0)]
        public double Speed { get; set; }

        // Indirizzo registro (default [akka.tcp://RepositoryActorSystem@localhost:8080])
        [Option(Default = Protocol.tcp)]
        public Protocol Protocol{ get; set; }
        
        [Option('h', Default = "localhost")]
        public string? Hostname { get; set; }
        
        [Option('p', Default = 8080)]
        public int Port { get; set; }

        public static int Run(DroneOptions options)
        {
            var config = SystemConfigs.DroneConfig;
            var host = options.Protocol switch
            {
                Protocol.tcp => new Host(options.Hostname!, options.Port, AkkaProtocol.TCP),
                Protocol.udp => new Host(options.Hostname!, options.Port, AkkaProtocol.UDP),
                _ => new Host(options.Hostname!, options.Port, AkkaProtocol.LOCAL)
            };
            var mission = new MissionPath(
                new Point2D(options.Xstart, options.Ystart),
                new Point2D(options.Xend, options.Yend),
                (float)options.Speed
                );
            
            using var system = ActorSystem.Create(config.SystemName, config.Config);
            var repository = new RepositoryProvider(system, config).TryConnectToExistent(host);
            if (repository is null)
            {
                Console.Error.WriteLine($"ERRORE! Impossibile collegarsi al repository {host}");
                return 1;
            }

            var missionAPI = new MissionSpawner(system, repository, SimpleMissionAPI.Factory()).SpawnHere(mission, "DroneA");
            if (missionAPI is null)
            {
                Console.Error.WriteLine($"ERRORE! Impossibile spawnare la missione!");
                return 1;
            }

            string? line;
            while ((line = Console.ReadLine()) != "cancel") {}
            missionAPI.Cancel();

            return 0;
        }
    }
}
