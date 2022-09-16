using Akka.Actor;

namespace Actors
{
    /*
 * Create an actor and a message type that gets shared between Deployer and DeployTarget
 * in a common DLL
 */
    /// <summary>
    /// Actor that just replies the message that it received earlier
    /// </summary>
    public class EchoActor : ReceiveActor, IWithTimers
    {
        public ITimerScheduler Timers { get; set; }

        public EchoActor()
        {
            Receive<Hello>(hello =>
            {
                Console.WriteLine("[{0}]: {1}", Sender, hello.Message);
                Sender.Tell(hello);
            });
        }
    }

    public class Hello
    {
        public Hello(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}