using Actors;
using Actors.DTO;
using Actors.Messages.External;
using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.ActorTests
{  
    /// <summary>
    /// Test sul servizio di notifica
    /// </summary>
    public class NotificationActorTest : TestKit
    {
        /// <summary>
        /// cielo libero 1:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo.
        /// </summary>
        [Fact]
        public void FreeSky1()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            

            Sys.Terminate();
        }
    }
}
