using Actors.Messages.StateChangeNotifier;
using Actors.Utils;
using Akka.Actor;
using System.Diagnostics;

using Actors.Messages.StateChangeNotifier;
using Actors.Utils;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Mission.ObserverMissionAPI
{
    /// <summary>
    /// Attore per l'osservazione dei cambiamenti di stato 
    /// di un drone. 
    /// 
    /// Immagazzina i cambiamenti di stato ricevuti in una coda e 
    /// li inoltra tutti assieme su richiesta. 
    /// 
    /// Nel caso riceve una richiesta e non ha ricevuto nuove 
    /// notifiche, aspetta di ricevere una notifica prima di 
    /// rispondere.
    /// </summary>
    internal class ObserverActor : ReceiveActor
    {
        /// <summary>
        /// Attore osservato
        /// </summary>
        private readonly IActorRef _observedNodeRef;

        /// <summary>
        /// Coda di notifiche non ancora consegnate. Non appena arriva una richiesta
        /// questa coda viene svuotata (in ordine di ID del messaggio)
        /// </summary>
        private IList<OrderedStateNotification> _notDeliveredNotifications = 
            new List<OrderedStateNotification>();

        /// <summary>
        /// (eventuale) mittente dell'ultima richiesta che non è stata ancora 
        /// servita
        /// </summary>
        private IActorRef? _lastPendingRequestSender;

        private readonly DebugLog _debugLog = new DebugLog(Context.System.Log);

        public static Props Props(IActorRef observed)
        {
            return Akka.Actor.Props.Create(() => new ObserverActor(observed));
        }

        public ObserverActor(IActorRef observed)
        {
            _observedNodeRef = observed;
            // _debugLog.ForcedDeactivate();

            Receive<OrderedStateNotification>(msg => OnReceive(msg));
            Receive<AskForNotifications>(msg => OnReceive(msg));
        }

        /// <summary>
        /// Ricezione di una notifica
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceive(OrderedStateNotification msg)
        {
            _notDeliveredNotifications.Add(msg);

            if (_lastPendingRequestSender is not null) DeliverNotifications();
        }

        /// <summary>
        /// Ricezione di una richiesta di nuove notifiche. 
        /// 
        /// Verrà servita non appena la lista delle notifiche 
        /// avrà almeno un elemento.
        /// </summary>
        /// <param name="msg"></param>
        private void OnReceive(AskForNotifications msg)
        {
            _lastPendingRequestSender = Sender;

            if (_notDeliveredNotifications.Any()) DeliverNotifications();
        }

        /// <summary>
        /// "svuota" la coda delle notifiche non consegnate
        /// e inviale al mittente dell'ultima richiesta.
        /// </summary>
        private void DeliverNotifications()
        {
            Debug.Assert(_lastPendingRequestSender is not null);

            // inoltro delle ultime notifiche ricevute
            // (ordinate per numero del messaggio)
            _lastPendingRequestSender.Tell(new Notifications(
                _notDeliveredNotifications
                    .OrderBy(notification => notification.MessageNumber)
                    .ToList()
                ));
            
            // pulizia lista e indicatore ultima richiesta
            _notDeliveredNotifications.Clear();
            _lastPendingRequestSender = null;
        }

        protected override void PreStart()
        {
            base.PreStart();

            // iscrizione all'observed
            Task<SubscribeConfirm> t = _observedNodeRef.Ask<SubscribeConfirm>(new SubscribeRequest(Self, true));
            t.ContinueWith(t =>
            {
                if (t.IsCompleted)
                    _debugLog.Info($"Successfully subscribed to {_observedNodeRef}");
                else
                    _debugLog.Error($"Error while subscribing to {_observedNodeRef}");
            });
        }

        protected override void PostStop()
        {
            base.PostStop();

            // disiscrizione dall'observed
            Task<UnsubscribeConfirm> t = _observedNodeRef.Ask<UnsubscribeConfirm>(new UnsubscribeRequest(Self));
            t.ContinueWith(t =>
            {
                if (t.IsCompleted)
                    _debugLog.Info($"Successfully un-subscribed from {_observedNodeRef}");
                else
                    _debugLog.Error($"Error while un-subscribing from {_observedNodeRef}");
            });
        }

        
    }
}
