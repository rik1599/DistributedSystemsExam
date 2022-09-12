using Actors.MissionPathPriority;
using Akka.Actor;

namespace Actors.MissionSets
{
    /// <summary>
    /// Strumento generico per gestire set di missioni attraverso 
    /// i riferimenti ai nodi.
    /// 
    /// Per prevenire duplicazioni, si occupa lui della creazione delle istanze.
    /// </summary>
    /// <typeparam name="M"></typeparam>
    public abstract class IMissionSet<M> where M : Mission
    {
        /// <summary>
        /// Tutte le missioni della collezione
        /// </summary>
        protected IDictionary<IActorRef, M> Missions { get; private set; } = new Dictionary<IActorRef, M>();

        /// <summary>
        /// Cerca una missione nel set attraverso il nome del suo nodo
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <returns>L'istanza della missione se esiste, altrimenti null</returns>
        public M? GetMission(IActorRef nodeRef)
        {
            if (Missions.ContainsKey(nodeRef))
                return Missions[nodeRef];
            return null;
        }

        /// <summary>
        /// Crea e aggiungi una nuova missione al set
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <param name="path"></param>
        public void AddMission(IActorRef nodeRef, MissionPath path)
        {
            if (!Missions.ContainsKey(nodeRef))
                Missions.Add(nodeRef, CreateMission(nodeRef, path));
        }

        /// <summary>
        /// Rimuovi una missione dal set
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <returns>L'istanza della missione eventualmente rimossa</returns>
        public M? RemoveMission(IActorRef nodeRef)
        {
            if (!Missions.ContainsKey(nodeRef))
                return null;

            var mission = Missions[nodeRef];
            Missions.Remove(nodeRef);
            return mission;
        }

        /// <summary>
        /// Crea una nuova missione
        /// </summary>
        /// <param name="nodeRef"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected abstract M CreateMission(IActorRef nodeRef, MissionPath path);

        public virtual ISet<IActorRef> GetNodes()
        {
            return Missions.Keys.ToHashSet();
        }

        public virtual ISet<M> GetMissions()
        {
            return Missions.Values.ToHashSet();
        }
    }
}
