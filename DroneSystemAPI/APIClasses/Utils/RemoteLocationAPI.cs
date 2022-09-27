﻿using Actors;
using Akka.Actor;

namespace DroneSystemAPI.APIClasses.Utils
{
    /// <summary>
    /// Strumento unico per:
    /// - reperire facilmente gli indirizzi di attori collocati su sistemi noti
    /// - spawnare attori (localmente o su sistemi remoti avviati)
    /// 
    /// </summary>
    public class RemoteLocationAPI
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new (0, 0, 10);
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Actor system locale utilizzato per interfacciarsi 
        /// con Akka.NET
        /// </summary>
        private ActorSystem _interfaceActorSystem;

        public RemoteLocationAPI(ActorSystem interfaceActorSystem) 
        {
            _interfaceActorSystem = interfaceActorSystem;
            _timeout = DEFAULT_TIMEOUT;
        }

        public RemoteLocationAPI(ActorSystem interfaceActorSystem, TimeSpan timeout) 
        {
            _interfaceActorSystem = interfaceActorSystem;
            _timeout = timeout;
        }

        public IActorRef? TryGetExistentActor(ActorSystem deployerSystem, Address systemAddress, string actorName)
        {
            try
            {
                var address = $"{systemAddress}/user/spawner/{actorName}";
                return _interfaceActorSystem.ActorSelection(address).ResolveOne(_timeout).Result;
            } catch (ActorNotFoundException)
            {
                return null;
            } catch (AggregateException)
            {
                return null;
            }

            // TODO: valutare se vale la pena propagare all'esterno
            //      le eccezioni per dare un idea del tipo di errore
        }

        public static IActorRef? SpawnLocally(ActorSystem localActorSystem, Props actorProps, string actorName)
        {
            try
            {
                return localActorSystem.ActorOf(actorProps, actorName);
            }
            catch (InvalidActorNameException)
            {
                return null;
            }
        }

        public IActorRef? SpawnRemote(ActorSystem deployerSystem, Address remoteAddress, Props actorProps, string actorName)
        {
            try
            {
                var remoteSpawner = _interfaceActorSystem
                        .ActorSelection($"{remoteAddress}/user/spawner")
                        .ResolveOne(_timeout).Result;

                var result = remoteSpawner.Ask(
                    new SpawnActorRequest(actorProps, actorName)).Result;

                if (result is IActorRef @ref) 
                    return @ref;
                else 
                    return null;
            }/*
            catch (ActorNotFoundException)
            {
                return null;
            }
            catch (AggregateException)
            {
                return null;
            } 
            catch (TimeoutException)
            {
                return null;
            } */
            catch (Exception e )
            {
                var x = e;
                return null;
            }
        }
    }
}
