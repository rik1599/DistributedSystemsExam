using Actors.DTO;
using Akka.Actor;

namespace Actors.Messages.StateChangeNotifier
{
    /// <summary>
    /// Generico messaggio del protocollo di notifica
    /// del cambio di stato (lo tengo in modo da facilitare
    /// operazioni di inoltro da parte dell'attore principale).
    /// </summary>
    public interface INotificationProtocolMessage { }
    
    /// <summary>
    /// Messaggio per la notifica di un cambiamento di stato. 
    /// 
    /// Al suo cambio di stato, l'attore del drone lo invia 
    /// allo state change notifier, che lo inoltra a tutti gli 
    /// iscritti.
    /// </summary>
    public class StateChangeNotification : INotificationProtocolMessage
    {      
        /// <summary>
        /// Stato del drone
        /// </summary>
        public DroneStateDTO NewState { get; }


        public StateChangeNotification(DroneStateDTO newState)
        {
            NewState = newState;
        }
    }

    /// <summary>
    /// Versione ordinata del messaggio di notifica del cambio stato
    /// </summary>
    public class OrderedStateNotification : StateChangeNotification
    {
        public int MessageNumber { get; }

        public OrderedStateNotification(DroneStateDTO newState, int msgNumber) : base(newState)
        {
            MessageNumber = msgNumber;
        }
    }

    /// <summary>
    /// Messaggio per iscriversi ad un servizio di notifica
    /// </summary>
    public class SubscribeRequest : INotificationProtocolMessage
    {
        public IActorRef ActorRef { get; }

        /// <summary>
        /// Indica se il nuovo iscritto vuole o
        /// meno che gli vengano inviati gli stati precedenti.
        /// </summary>
        public bool SendPrecedent { get; } = true;

        public SubscribeRequest(IActorRef actorRef)
        {
            ActorRef = actorRef;
        }

        public SubscribeRequest(IActorRef actorRef, bool sendPrecedent) : this(actorRef)
        {
            SendPrecedent = sendPrecedent;
        }
    }

    public class SubscribeConfirm : INotificationProtocolMessage
    {
        
    }

    /// <summary>
    /// Messaggio per disiscriversi da un certo servizio 
    /// di notifica.
    /// </summary>
    public class UnsubscribeRequest : INotificationProtocolMessage
    {
        public IActorRef ActorRef { get; }

        public UnsubscribeRequest(IActorRef actorRef)
        {
            ActorRef = actorRef;
        }
    }

    public class UnsubscribeConfirm : INotificationProtocolMessage
    {

    }
}
