using Actors.DTO;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI
{
    /// <summary>
    /// API più avanzata, che consente di ricevere (tramite 
    /// un'attore osservatore intermedio) una serie di aggiornamenti
    /// sullo stato della missione.
    /// 
    /// All'interfaccia <code>IMissionAPI</code> si aggiunge un 
    /// nuovo metodo <code>AskForNotifications()</code>, che consente
    /// di mettersi in attesa di nuove notifiche.
    /// 
    /// TODO: TEST!!!
    /// </summary>
    public class ObserverMissionAPI : IMissionAPI
    {
        private readonly IMissionAPI _baseAPI;


        /// <summary>
        /// Attore che uso per ricevere notifiche
        /// </summary>
        private readonly IActorRef _observerRef;

        internal ObserverMissionAPI(IMissionAPI baseAPI, IActorRef observerActor)
        {
            _baseAPI = baseAPI;
            _observerRef = observerActor;
        }

        public Task Cancel()
        {
            return _baseAPI.Cancel();
        }

        public Task<DroneStateDTO> GetCurrentStatus()
        {
            return _baseAPI.GetCurrentStatus();
        }

        public IActorRef GetDroneRef()
        {
            return _baseAPI.GetDroneRef();
        }

        /// <summary>
        /// Ricevi una lista ordinata di notifiche rappresentanti 
        /// degli eventi rilevanti per la missione (ad es., cambio di stato)
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<DroneStateDTO>> AskForUpdates()
        {
            try
            {
                var t = await _observerRef
                    .Ask<Notifications>(new AskForNotifications());

                return t.NewNotifications
                    .Select(notification => notification.NewState)
                    .ToList();
            }
            catch (Exception)
            {
                throw new MissionIsUnreachableException();
            }
        }

        public static IMissionAPIFactory Factory(ActorSystem localSystem)
            => new ObserverMissionAPIFactory(localSystem);
    }

    public class ObserverMissionAPIFactory : IMissionAPIFactory
    {
        /// <summary>
        /// Sistema locale (dove spawnare gli attori per le notifiche)
        /// </summary>
        private readonly ActorSystem _localSystem;

        public ObserverMissionAPIFactory(ActorSystem localSystem)
        {
            _localSystem = localSystem;
        }

        public IMissionAPI GetMissionAPI(IActorRef nodeRef)
        {
            return GetObserverMissionAPI(nodeRef);
        }

        public ObserverMissionAPI GetObserverMissionAPI(IActorRef nodeRef)
        {
            return new ObserverMissionAPI(

                // API per gestire le funzionalità di base
                new SimpleMissionAPI.SimpleMissionAPI(nodeRef),

                // attore che gestisce la ricezione delle notifiche
                _localSystem.ActorOf(ObserverActor.Props(nodeRef))
                );
        }
    }
}
