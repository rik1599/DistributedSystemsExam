using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actors.Mission
{
    /// <summary>
    /// Priorità di una missione 
    /// </summary>
    public class Priority : IComparable<Priority>
    {
        public Double MetricValue { get; private set; }

        /// <summary>
        /// L'identificatore del nodo. In caso di parità di metrica 
        /// la priorità più alta ce l'ha il nodo con ID minore.
        /// </summary>
        public IActorRef? NodeRef { get; private set; }


        public static Priority InfinitePriority = new InfinitePriority();
        public static Priority NullPriority = new NullPriority();


        public int CompareTo(Priority? other)
        {
            throw new NotImplementedException();
        }

        internal Priority(double metricValue, IActorRef? nodeRef)
        {
            MetricValue = metricValue;
            NodeRef = nodeRef;
        }
,     }

    internal class InfinitePriority : Priority
    {
        internal InfinitePriority() : base(Double.MaxValue, null)
        {
        }
    }

    internal class NullPriority : Priority
    {
        internal NullPriority() : base(Double.MinValue, null)
        {
        }
    }


}
