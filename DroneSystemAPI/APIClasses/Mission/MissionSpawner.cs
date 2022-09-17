﻿using Actors;
using Actors.MissionPathPriority;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Register;
using DroneSystemAPI.APIClasses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Mission
{
    public class MissionSpawner
    {
        private readonly ActorSystem _localSystem;
        private readonly DroneSystemConfig _config = new DroneSystemConfig();

        /// <summary>
        /// Il registro indicato alle misioni generate
        /// </summary>
        private readonly RegisterAPI _register;

        /// <summary>
        /// Lo strumento da utilizzare per generare le missioni
        /// </summary>
        private readonly IMissionAPIFactory _missionAPIFactory;

        public MissionSpawner(ActorSystem localSystem, RegisterAPI register, IMissionAPIFactory missionAPIFactory)
        {
            _localSystem = localSystem;
            _register = register;
            _missionAPIFactory = missionAPIFactory;
        }

        public MissionSpawner(ActorSystem localSystem, RegisterAPI register, IMissionAPIFactory missionAPIFactory, DroneSystemConfig config) 
            : this(localSystem, register, missionAPIFactory)
        {
            _config = config;
        }

        /// <summary>
        /// Prova a collegarti ad una missione esistente .
        /// </summary>
        /// <param name="host">host dove cercare il drone</param>
        /// <returns></returns>
        public IMissionAPI? TryConnectToExistent(Host host)
        {
            ActorProvider actorProvider = new ActorProvider();
            var actorRef = actorProvider.TryGetExistentActor(
                _localSystem,
                Address.Parse(host.GetSystemAddress(_config.DroneActorName)),
                _config.DroneSystemName);

            return (actorRef is null) ? null : _missionAPIFactory.GetMissionAPI(actorRef);
        }


        public IMissionAPI SpawnHere(MissionPath missionPath, String missionName)
        {
            ActorProvider actorProvider = new ActorProvider();
            var actorRef = actorProvider.SpawnLocally(
                _localSystem,
                DroneActor.Props(_register.ActorRef, missionPath),
                missionName);

            return _missionAPIFactory.GetMissionAPI(actorRef);
        }


        public IMissionAPI SpawnRemote(Host host, MissionPath missionPath, String missionName)
        {
            ActorProvider actorProvider = new ActorProvider();
            var actorRef = actorProvider.SpawnRemote(
                _localSystem,
                Address.Parse(host.GetSystemAddress(_config.DroneSystemName)),
                DroneActor.Props(_register.ActorRef, missionPath),
                missionName);

            return _missionAPIFactory.GetMissionAPI(actorRef);
        }

    }
}
