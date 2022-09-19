using Actors.DroneStates;
using Actors.Messages.StateChangeNotifier;
using Akka.Actor;

namespace Actors.StateChangeNotifier
{
    /// <summary>
    /// Strumento che l'attore può usare per gestire in modo 
    /// trasparente il protocollo di gestione della notifica
    /// per il cambio di stato.
    /// 
    /// Fornisce:
    /// -   un metodo per gestire i messaggi relativi al protocollo
    /// -   un Visitor utilizzabile dallo stato per 
    ///     inviare agli iscritti la notifica del cambio di stato
    /// -   una gestione trasparente (tramite attore figlio) di tutto
    ///     il protocollo (quindi della gestione della lista degli 
    ///     iscritti, dell'invio ad essi delle notifiche, ecc.)
    /// </summary>
    internal class NotificationProtocol
    {
        private readonly IActorRef _notificationServiceActor;

        public NotificationProtocol(DroneActorContext context, IReadOnlySet<IActorRef>? initialSubscribed = null)
        {
            // spawn attore figlio per gestire il servizio di notifica
            _notificationServiceActor = context.Context.ActorOf(
                StateChangeNotifierActor.Props(
                    context.Context.Self,
                    initialSubscribed ?? new HashSet<IActorRef>()
                    ), 
                "notification-service"
                );
        }

        /// <summary>
        /// Inoltro dei messaggi del protocollo di notifica
        /// all'attore apposito.
        /// </summary>
        /// <param name="msg"></param>
        public void OnReceive(INotificationProtocolMessage msg)
        {
            _notificationServiceActor.Forward(msg);
        }

        /// <summary>
        /// Strumento utilizzabile dagli stati per comunicare
        /// il proprio cambio di stato. 
        /// </summary>
        /// <returns></returns>
        public IDroneStateVisitor GetStateChangeNotifierVisitor()
        {
            return new StateChangeNotifierVisitor(_notificationServiceActor);
        }

    }
}
