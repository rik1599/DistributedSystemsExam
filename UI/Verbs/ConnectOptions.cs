using CommandLine;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI;
using DroneSystemAPI.APIClasses.Mission;
using Actors.DTO;

namespace UI.Verbs
{
    [Verb("connect", HelpText = "Connect to a remote drone")]
    internal class ConnectOptions
    {
        // Indirizzo registro (default [akka.tcp://RepositoryActorSystem@localhost:8080])
        [Option(Default = Protocol.tcp)]
        public Protocol Protocol { get; set; }

        [Option('h', Default = "localhost")]
        public string? Hostname { get; set; }

        [Option('p', Required = true)]
        public int Port { get; set; }

        public static int Run(ConnectOptions options)
        {
            var config = SystemConfigs.DroneConfig;
            config.SystemName = "ConnectActorSystem";

            var host = options.Protocol switch
            {
                Protocol.tcp => new Host(options.Hostname!, options.Port, AkkaProtocol.TCP),
                Protocol.udp => new Host(options.Hostname!, options.Port, AkkaProtocol.UDP),
                _ => new Host(options.Hostname!, options.Port, AkkaProtocol.LOCAL)
            };

            using var system = ActorSystem.Create(config.SystemName, config.Config);

            var obsAPI = (ObserverMissionAPI?)new MissionProvider(system).TryConnectToExistent(host, "DroneA");
            if (obsAPI is null)
            {
                Console.Error.WriteLine("Errore! Impossibile connettersi al drone remoto");
                return 1;
            }

            IList<DroneStateDTO> notifications = new List<DroneStateDTO>();
            do
            {
                var newNotifications = obsAPI.AskForUpdates().Result;
                foreach (var n in newNotifications)
                {
                    notifications.Add(n);
                }
            } while ((notifications.Last() as ExitStateDTO) == null);

            system.WhenTerminated.RunSynchronously();
            return 0;
        }
    }
}
