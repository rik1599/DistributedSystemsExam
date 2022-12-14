using Actors.MissionPathPriority;

namespace Actors.Messages.External
{
    /// <summary>
    /// Messaggio di presentazione del nodo alla rete
    /// </summary>
    public class ConnectRequest
    {
        public MissionPath Path { get; }

        public ConnectRequest(MissionPath path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Messaggio di risposta dei nodi al nodo in rete
    /// </summary>
    public class ConnectResponse
    {
        public MissionPath Path { get; }

        public ConnectResponse(MissionPath path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Messaggio di risposta di un nodo in volo
    /// </summary>
    public class FlyingResponse
    {
        public MissionPath Path { get; }

        public FlyingResponse(MissionPath path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Messaggio per lo scambio delle metriche
    /// </summary>
    public class MetricMessage
    {
        public Priority Priority { get; }
        public int RelativeRound { get; }

        public MetricMessage(Priority priority, int relativeRound)
        {
            Priority = priority;
            RelativeRound = relativeRound;
        }
    }

    /// <summary>
    /// Messaggio di attesa. Chi lo riceve attende il mittente per il round corrente
    /// </summary>
    public class WaitMeMessage
    {

    }

    /// <summary>
    /// Messaggio di uscita del nodo dalla rete
    /// </summary>
    public class ExitMessage
    {

    }

    public class MissionFinishedMessage : ExitMessage
    {

    }

    public class MissionCanceledMessage : ExitMessage
    {

    }
}
