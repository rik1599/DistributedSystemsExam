using Actors;
using Actors.MissionPathPriority;
using Akka.Actor;
using DroneSystemAPI.APIClasses.Mission;
using DroneSystemAPI.APIClasses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses
{
    public class DroneDeliverySystemAPI
    {
        private readonly ActorSystem _interfaceActorSystem;

        /// <summary>
        /// L'indirizzo del repository
        /// </summary>
        public IActorRef? RepositoryAddress { get; private set; }

        /// <summary>
        /// Il nome di tutti gli actor system che compongono questo 
        /// sistema (ad eccezione di quelli "di interfaccia").
        /// </summary>
        public String SystemName { get; }

        /// <summary>
        /// Il nome che mi attendo abbia il nodo repository
        /// </summary>
        public String RepositoryActorName { get; }

        public DroneDeliverySystemAPI(ActorSystem interfaceActorSystem, string systemName, string repositoryActorName)
        {
            _interfaceActorSystem = interfaceActorSystem;
            SystemName = systemName;
            RepositoryActorName = repositoryActorName;
        }

        /// <summary>
        /// Verifica se una locazione remota è attiva
        /// </summary>
        /// <param name="remoteHost"></param>
        /// <returns></returns>
        public bool VerifyLocation(Host remoteHost)
        {
            try
            {
                RemoteLocationAPI remoteLocation = new RemoteLocationAPI(
                _interfaceActorSystem,
                new DeployPointDetails(remoteHost, SystemName));

                return remoteLocation.Verify();
            } catch (Exception)
            {
                return false;
            }
        }

        public bool HasRepository()
        {
            return RepositoryAddress is not null;
        }

        /// <summary>
        /// Dispiega un un repository su una locazione remota
        /// </summary>
        /// <param name="remoteHost">Indirizzo della locazione (inizializzata)</param>
        /// <exception cref="Exception"></exception>
        public void DeployRepository(Host remoteHost)
        {
            RepositoryAddress = null;

            RemoteLocationAPI remoteLocation = new RemoteLocationAPI(
                _interfaceActorSystem,
                new DeployPointDetails(remoteHost, SystemName));

            if (!remoteLocation.Verify())
                throw new Exception($"Impossibile dispiegare il registro. " +
                    $"La locazione {remoteHost} non è valida");

            RepositoryAddress = remoteLocation
                .SpawnActor(DronesRepositoryActor.Props(), RepositoryActorName);

            if (RepositoryAddress is null)
                throw new Exception(
                    $"Il dispiegamento del registro su {remoteHost} non è andata a buon fine.");

            // TODO: costruisci un appropriato sistema di eccezioni
        }

        public void SetRepository(Host remoteHost)
        {
            RepositoryAddress = null;

            RemoteLocationAPI remoteLocation = new RemoteLocationAPI(
                _interfaceActorSystem,
                new DeployPointDetails(remoteHost, SystemName));

            if (!remoteLocation.Verify())
                throw new Exception($"Impossibile dispiegare il registro. " +
                    $"La locazione {remoteHost} non è valida");

            RepositoryAddress = remoteLocation.GetActorRef(RepositoryActorName);

            if (RepositoryAddress is null)
                throw new Exception(
                    $"Il dispiegamento del registro su {remoteHost} non è andata a buon fine.");
        }

        public IMissionAPI SpawnMission(Host remoteHost, 
            MissionPath missionPath, 
            string missionName, 
            IMissionAPIFactory missionAPIFactory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteHost"></param>
        /// <param name="missionName"></param>
        /// <param name="missionAPIFactory"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IMissionAPI ConnectToMission(Host remoteHost, 
            string missionName,
            IMissionAPIFactory missionAPIFactory)
        {
            throw new NotImplementedException();
        }

    }
}
