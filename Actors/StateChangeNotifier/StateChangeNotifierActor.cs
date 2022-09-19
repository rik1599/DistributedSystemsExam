using Actors.Messages.StateChangeNotifier;
using Akka.Actor;

namespace Actors.StateChangeNotifier
{
    /// <summary>
    /// Attore che gestisce la notifica di un cambio di stato. 
    /// 
    /// Un attore base <code>_mainActor</code> gli comunica quando
    /// cambia stato, lui provvede a notificare un gruppo di iscritti.
    /// 
    /// L'attore base probabilmente gli inoltra anche tutti i messaggi 
    /// del protocollo utilizzando <code>Forward(msg)</code>.
    /// </summary>
    internal class StateChangeNotifierActor : ReceiveActor
    {
        /// <summary>
        /// Riferimento all'attore principale. Ogni messaggio che invio
        /// lo invio a nome suo.
        /// 
        /// Mi aspetto anche mi vengano inotrati da lui i messaggi (tramite
        /// <code>Forward(msg))</code>
        /// </summary>
        private readonly IActorRef _mainActor;

        /// <summary>
        /// Attori iscritti al servizio di notifica
        /// </summary>
        private readonly ISet<IActorRef> _subscribed;

        /// <summary>
        /// Lista di tutti gli stati precedenti 
        /// (si tiene in modo che possa essere comunicata
        /// ai nuovi iscritti che lo richiedono)
        /// </summary>
        private readonly IList<StateChangeNotification> _precStates = 
            new List<StateChangeNotification>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainActor">Attore principale a cui fa riferimento</param>
        /// <param name="initialSubscribed">Lista iniziale di iscritti</param>
        /// <returns></returns>
        public static Props Props(IActorRef mainActor, IReadOnlySet<IActorRef>? initialSubscribed = null)
        {
            return Akka.Actor.Props.Create(() => 
            new StateChangeNotifierActor(
                mainActor, 
                initialSubscribed ?? new HashSet<IActorRef>())
            );
        }

        public StateChangeNotifierActor(IActorRef mainActor, IReadOnlySet<IActorRef> initialSubscribed)
        {
            _mainActor = mainActor;
            _subscribed = initialSubscribed.ToHashSet();

            Receive<StateChangeNotification>(msg => OnReceive(msg));
            Receive<SubscribeRequest>(msg => OnReceive(msg));
            Receive<UnsubscribeRequest>(msg => OnReceive(msg));
        }

        private void OnReceive(StateChangeNotification msg)
        {
            if (Sender == _mainActor)
            {
                var ordMsg = new OrderedStateNotification(
                        msg.NewState, _precStates.Count);

                _precStates.Add(ordMsg);

                foreach (IActorRef s in _subscribed)
                {
                    s.Tell(ordMsg, _mainActor);
                }
            }
        }

        private void OnReceive(SubscribeRequest msg)
        {
            if (msg.ActorRef == _mainActor) return;

            // se non è già iscritto, lo aggiungo alla lista
            if (!_subscribed.Contains(msg.ActorRef))  _subscribed.Add(msg.ActorRef);

            // invio conferma (anche se è già iscritto)
            Sender.Tell(new SubscribeConfirm(), _mainActor);

            // se richiesto, invio tutte le notifiche precedenti
            if (msg.SendPrecedent) 
                foreach(var notification in _precStates)
                {
                    msg.ActorRef.Tell(notification, _mainActor);
                }
        }

        private void OnReceive(UnsubscribeRequest msg)
        {
            _subscribed.Remove(msg.ActorRef);
            Sender.Tell(new UnsubscribeConfirm(), _mainActor);
        }
    }
}
