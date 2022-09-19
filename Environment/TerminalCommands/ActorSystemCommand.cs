using Akka.Actor;
using DroneSystemAPI;

namespace UI.TerminalCommands
{
    internal class ActorSystemCommand : ITerminalCommand
    {
        private readonly SystemConfigs _configs;

        public ActorSystemCommand(SystemConfigs configs)
        {
            _configs = configs;
        }

        public void Execute()
        {
            using var system = ActorSystem.Create(_configs.SystemName, _configs.Config);
            Console.ReadKey();
        }
    }
}
