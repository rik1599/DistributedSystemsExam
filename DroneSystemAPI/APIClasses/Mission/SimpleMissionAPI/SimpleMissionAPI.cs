using Actors.DTO;
using Actors.Messages.User;
using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DroneSystemAPI.APIClasses.Mission.SimpleMissionAPI
{
    public class SimpleMissionAPI : IMissionAPI
    {
        public static readonly TimeSpan DEFAULT_TIMEOUT = new TimeSpan(0, 0, 10);


        private readonly TimeSpan _timeout = DEFAULT_TIMEOUT;
        private readonly IActorRef _nodeRef;

        public SimpleMissionAPI(IActorRef nodeRef)
        {
            _nodeRef = nodeRef;
        }

        public SimpleMissionAPI(IActorRef nodeRef, TimeSpan timeout) : this(nodeRef)
        {
            _timeout = timeout;
        }

        public IActorRef GetDroneRef() => _nodeRef;

        public async Task<DroneStateDTO> GetCurrentStatus()
        {
            Task<GetStatusResponse> t = GetDroneRef()
                .Ask<GetStatusResponse>(new GetStatusRequest(), _timeout);

            t.Wait();
            if (t.IsFaulted || !t.IsCompleted)
            {
                throw new MissionIsUnreachableException();
            }

            return t.Result.StateDTO;
        }

        public Task Cancel()
        {
            throw new NotImplementedException();
        }

        public static IMissionAPIFactory Factory() => new SimpleMissionAPIFactory();
    }

    internal class SimpleMissionAPIFactory : IMissionAPIFactory
    {
        public IMissionAPI GetMissionAPI(IActorRef nodeRef)
        {
            return new SimpleMissionAPI(nodeRef);
        }
    }
}
