using Actors;
using Actors.MissionPathPriority;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Utils;

namespace DroneSystemAPI.APIClasses.Mission
{
    [Obsolete("Not used any more", true)]
    public class MissionProvider
    {
        private readonly ActorSystem _localSystem;
        private readonly SystemConfigs _config = SystemConfigs.DroneConfig;

        /// <summary>
        /// Lo strumento da utilizzare per generare le missioni
        /// </summary>
        private readonly IMissionAPIFactory _missionAPIFactory;

        public MissionProvider(ActorSystem localSystem)
        {
            _localSystem = localSystem;
            _missionAPIFactory = ObserverMissionAPI.ObserverMissionAPI.Factory(_localSystem);
        }

        public MissionProvider(ActorSystem localSystem, SystemConfigs config) : this(localSystem)
        {
            _config = config;
        }

        public MissionProvider(ActorSystem localSystem, SystemConfigs config, IMissionAPIFactory missionAPIFactory) : this(localSystem, config)
        {
            _missionAPIFactory = missionAPIFactory;
        }

        /// <summary>
        /// Prova a collegarti ad una missione esistente .
        /// </summary>
        /// <param name="host">host dove cercare il drone</param>
        /// <returns></returns>
        public IMissionAPI? TryConnectToExistent(Host host, string missionName)
        {
            var actorRef = new RemoteLocationAPI(_localSystem, new DeployPointDetails(host, _config.SystemName))
                .GetActorRef(missionName);

            return (actorRef is null) ? null : _missionAPIFactory.GetMissionAPI(actorRef);
        }
    }

    [Obsolete("Not used any more", true)]
    public class MissionSpawner
    {
        private readonly ActorSystem _localSystem;
        private readonly SystemConfigs _config = SystemConfigs.DroneConfig;

        /// <summary>
        /// Il registro indicato alle misioni generate
        /// </summary>
        private readonly IActorRef _register;

        /// <summary>
        /// Lo strumento da utilizzare per generare le missioni
        /// </summary>
        private readonly IMissionAPIFactory _missionAPIFactory;

        public MissionSpawner(ActorSystem localSystem, IActorRef register, IMissionAPIFactory missionAPIFactory)
        {
            _localSystem = localSystem;
            _register = register;
            _missionAPIFactory = missionAPIFactory;
        }

        public MissionSpawner(ActorSystem localSystem, IActorRef register, IMissionAPIFactory missionAPIFactory, SystemConfigs config) 
            : this(localSystem, register, missionAPIFactory)
        {
            _config = config;
        }

        public IMissionAPI? SpawnHere(MissionPath missionPath, string missionName)
        {
            var actorRef = _localSystem.ActorOf(
                DroneActor.Props(_register, missionPath),
                missionName);

            return actorRef is null ? null : _missionAPIFactory.GetMissionAPI(actorRef);
        }


        public IMissionAPI? SpawnRemote(Host host, MissionPath missionPath, string missionName)
        {
            var actorRef = new RemoteLocationAPI(_localSystem, new DeployPointDetails(host, _config.SystemName))
                .SpawnActor(DroneActor.Props(_register, missionPath), missionName);

            return actorRef is null ? null : _missionAPIFactory.GetMissionAPI(actorRef);
        }

    }
}
