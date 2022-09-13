using Actors.Messages.Internal;
using Actors.MissionPathPriority;
using Actors.MissionSets;
using Akka.Actor;

namespace Actors
{
    /// <summary>
    /// Strumento per il tracciamento delle missioni in volo. 
    /// Permette di:
    /// -   registrare la partenza di una missione in attesa
    /// -   cancellare una missione
    /// -   inviare al nodo un messaggio ogni qual volta
    ///     una delle missioni in volo raggiunge un punto per 
    ///     il quale la mia partenza è sicura.
    ///     
    /// NOTA: chiamare OnReceive(InternalFlyIsSafeMessage) per
    /// liberare il flying set.
    /// </summary>
    public class FlyingMissionsMonitor
    {
        private readonly Mission _thisMission;

        private readonly FlyingSet _flyingSet;

        /// <summary>
        /// Strumento che l'attore usa schedulare l'invio di
        /// un messaggio a se stesso.
        /// </summary>
        private readonly ITimerScheduler _timers;

        public FlyingMissionsMonitor(Mission thisMission, FlyingSet flyingSet, ITimerScheduler timers)
        {
            _thisMission = thisMission;
            _flyingSet = flyingSet;
            _timers = timers;
        }

        /// <summary>
        /// Prendi una missione ferma e registra la sua partenza.
        /// </summary>
        /// <param name="mission"></param>
        public void MakeMissionFly(WaitingMission mission)
        {
            _flyingSet.AddMission(mission.NodeRef, mission.Path);
            FlyingMission flyingMission = _flyingSet.GetMission(mission.NodeRef)!;

            // todo: "invalida" l'istanza di WaitingMission in modo che non
            // possa più essere utilizzata

            var safeWaitTime = flyingMission.GetRemainingTimeForSafeStart(_thisMission);

            if (safeWaitTime > TimeSpan.Zero)
            {
                // pianifico l'invio a me stesso di un messaggio (che arriva quando si libera la tratta)
                _timers.StartSingleTimer(flyingMission,
                    new InternalFlyIsSafeMessage(flyingMission.NodeRef),
                    safeWaitTime
                    );
            }
            else
            {
                _timers.StartSingleTimer(flyingMission,
                    new InternalFlyIsSafeMessage(flyingMission.NodeRef),
                    TimeSpan.Zero
                    );
            }   
        }

        /// <summary>
        /// Chiama questo metodo per liberare il flying set da
        /// missioni che hanno raggiunto un punto safe.
        /// </summary>
        /// <param name="msg"></param>
        public void OnReceive(InternalFlyIsSafeMessage msg)
        {
            _flyingSet.RemoveMission(msg.SafeMissionNodeRef);
        }

        /// <summary>
        /// Cancella una certa missione in volo e annulla 
        /// l'invio del messaggio verso me stesso che è stato
        /// schedulato.
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <returns>True se la missione viene cancellata, false se non era presente.</returns>
        public bool CancelMission(IActorRef nodeRef)
        {
            FlyingMission? flyingMission = _flyingSet.GetMission(nodeRef);

            if (flyingMission != null)
            {
                // annullo l'invio del messaggio pianificato
                _timers.Cancel(flyingMission);

                // libero il flying set
                return _flyingSet.RemoveMission(nodeRef) != null;
            }

            return false;
        }

        public ISet<FlyingMission> GetFlyingMissions()
        {
            return _flyingSet.GetMissions();
        }
    }
}
