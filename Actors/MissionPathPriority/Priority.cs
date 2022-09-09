using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.MissionPathPriority
{
    /// <summary>
    /// Priorità di una missione; è rappresentata da una metrica. 
    /// A parita di metrica, per il confronto si considera 
    /// l'identificatore del nodo.
    /// 
    /// Non possono esistere due noti di priorità uguale.
    /// </summary>
    public class Priority : IComparable<Priority>
    {
        /// <summary>
        /// Misura della priorità di un nodo. E' un numero reale.
        /// </summary>
        public Double MetricValue { get; private set; }

        /// <summary>
        /// L'identificatore del nodo. In caso di parità di metrica 
        /// la priorità più alta ce l'ha il nodo con ID minore.
        /// </summary>
        public IActorRef? NodeRef { get; private set; }

        /// <summary>
        /// Priorità di un nodo che ha precedenza su tutti gli altri. 
        /// I nodi in volo hanno priorità infinita, così come i nodi
        /// che non hanno ancora condiviso la propria metrica.
        /// 
        /// Non confrontare con altre priorità infinite.
        /// </summary>
        public static Priority InfinitePriority { get { return new InfinitePriority(); } }

        /// <summary>
        /// Priorità di un nodo che vuole perdere tutte le negoziazioni.
        /// E' la priorità condivisa (in caso di richiesta) da un nodo 
        /// quando è ancora in fase di inizializzazione.
        /// 
        /// Non confrontare con altre priorità nulle.
        /// </summary>
        public static Priority NullPriority { get { return new NullPriority(); } }

        /// <summary>
        /// Confronta due priorità. Il confronto ritorna un numero positivo 
        /// se questa priorità è maggiore, un numero negativo se è minore. 
        /// Due priorità non possono mai essere uguali.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(Priority? other)
        {
            Debug.Assert(other != null);
            Debug.Assert(!this.Equals(other));
            Debug.Assert(NodeRef != null);
            Debug.Assert(!NodeRef!.Equals(other.NodeRef));

            if (MetricValue.CompareTo(other.MetricValue) != 0) return MetricValue.CompareTo(other.MetricValue);

            // in caso di parità di metrica vince il nodo
            // con identificatore più piccolo
            return - NodeRef.CompareTo(other.NodeRef);
        }

        internal Priority(double metricValue, IActorRef? nodeRef)
        {
            MetricValue = metricValue;
            NodeRef = nodeRef;
        }
    }

    internal class InfinitePriority : Priority
    {
        internal InfinitePriority() : base(Double.MaxValue, null)
        {
        }

        public override int CompareTo(Priority? other)
        {
            Debug.Assert(!(other is InfinitePriority));
            return +1;
        }
    }

    internal class NullPriority : Priority
    {
        internal NullPriority() : base(Double.MinValue, null)
        {
        }

        public override int CompareTo(Priority? other)
        {
            Debug.Assert(!(other is NullPriority));
            return -1;
        }
    }


}
