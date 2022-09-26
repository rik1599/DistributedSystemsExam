using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors
{
    /// <summary>
    /// Attore che gestisce lo spawn dei nodi e che applica
    /// una politica di supervisione consona.
    /// </summary>
    public class SpawnerActor : ReceiveActor
    {
        private readonly ILoggingAdapter _logger = Context.GetLogger();

        public SpawnerActor()
        {
            Receive<SpawnActorRequest>((msg) =>
            {
                IActorRef child = Context.ActorOf(msg.ActorProps, msg.ActorName);
                Sender.Tell(child);
            });
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 0,
                withinTimeRange: Timeout.InfiniteTimeSpan, 
                localOnlyDecider: ex =>
                {
                    _logger.Error($"A drone mission failed due to exception: {ex}");
                    return Directive.Stop;

                    /*
                    switch (ex)
                    {
                         case ArithmeticException ae:
                            return Directive.Resume;
                        case NullReferenceException nre:
                            return Directive.Restart;
                        case ArgumentException are:
                            return Directive.Stop;
                        default:
                            return Directive.Escalate; 
                    }
                    */
                });
        }
    }

    public class SpawnActorRequest
    {
        public Props ActorProps { get; }
        public string ActorName { get; }

        public SpawnActorRequest(Props actorProps, string actorName)
        {
            ActorProps = actorProps;
            ActorName = actorName;
        }
    }
}
