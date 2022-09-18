using Actors;
using Actors.DTO;
using Actors.Messages.StateChangeNotifier;
using Actors.MissionPathPriority;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using MathNet.Spatial.Euclidean;


namespace UnitTests.ActorTests
{  
    /// <summary>
    /// Test sul servizio di notifica
    /// </summary>
    public class NotificationActorTest : TestKit
    {
        /// <summary>
        /// test ricezione notifiche:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo. 
        /// Io risulto registrato sin dall'inizio al servizio di notifica
        /// </summary>
        [Fact]
        public void GetNotifications()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA, TestActor), "droneProva");

            // ora mi aspetto di ricevere una sequenza di stati
            var m0 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(0, m0.MessageNumber);
            Assert.IsType<InitStateDTO>(m0.NewState);

            var m1 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(1, m1.MessageNumber);
            Assert.IsType<NegotiateStateDTO>(m1.NewState);

            var m2 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(2, m2.MessageNumber);
            Assert.IsType<FlyingStateDTO>(m2.NewState);

            var m3 = ExpectMsgFrom<OrderedStateChangeNotification>(subject, new TimeSpan(0,0,7));
            Assert.Equal(3, m3.MessageNumber);
            Assert.IsType<ExitStateDTO>(m3.NewState);

            Sys.Terminate();
        }

        /// <summary>
        /// test iscrizione:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo. 
        /// Io mi registro dopo al servizio di notifica (e richiedo tutti i DTO)
        /// </summary>
        [Fact]
        public void NotifySubscribe()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(25, 25), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA), "droneProva");

            // richiedo di registrarmi al servizio di notifica
            // (specificando che voglio ricevere i vecchi stati)
            subject.Tell(new SubscribeRequest(TestActor, true));

            // mi aspetto una conferma
            ExpectMsgFrom<SubscribeConfirm>(subject);

            // ora mi aspetto di ricevere una sequenza di stati
            var m0 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(0, m0.MessageNumber);
            Assert.IsType<InitStateDTO>(m0.NewState);

            var m1 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(1, m1.MessageNumber);
            Assert.IsType<NegotiateStateDTO>(m1.NewState);

            var m2 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(2, m2.MessageNumber);
            Assert.IsType<FlyingStateDTO>(m2.NewState);

            var m3 = ExpectMsgFrom<OrderedStateChangeNotification>(subject, new TimeSpan(0, 0, 7));
            Assert.Equal(3, m3.MessageNumber);
            Assert.IsType<ExitStateDTO>(m3.NewState);

            Sys.Terminate();
        }

        /// <summary>
        /// test disiscrizione:
        /// 
        /// un drone spawna, non conosce nessun nodo, va in volo. 
        /// Io risulto registrato sin dall'inizio al servizio di notifica, 
        /// ma ad un certo punto mi disiscrivo.
        /// </summary>
        [Fact]
        public void NotifyUnsubscribe()
        {
            var missionA = new MissionPath(Point2D.Origin, new Point2D(50, 50), 10.0f);

            // voglio simulare una situazione in cui il nodo all'inizio è solo
            var nodes = new HashSet<IActorRef> { };
            var subject = Sys.ActorOf(DroneActor.Props(nodes, missionA, TestActor), "droneProva");

            // ora mi aspetto di ricevere una sequenza di stati
            var m0 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(0, m0.MessageNumber);
            Assert.IsType<InitStateDTO>(m0.NewState);

            var m1 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(1, m1.MessageNumber);
            Assert.IsType<NegotiateStateDTO>(m1.NewState);

            var m2 = ExpectMsgFrom<OrderedStateChangeNotification>(subject);
            Assert.Equal(2, m2.MessageNumber);
            Assert.IsType<FlyingStateDTO>(m2.NewState);

            // richiedo di disiscrivermi dal servizio di notifica
            subject.Tell(new UnsubscribeRequest(TestActor));
            ExpectMsgFrom<UnsubscribeConfirm>(subject);

            // NON mi aspetto altri messaggi
            ExpectNoMsg(new TimeSpan(0, 0, 7));

            Sys.Terminate();
        }


    }
}
