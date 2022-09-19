using Akka.Actor;
using DroneSystemAPI;

namespace UI.TerminalCommands
{
    internal abstract class ConnectCommand : ITerminalCommand
    {
        protected SystemConfigs Configs { get; }

        public ConnectCommand(SystemConfigs configs)
        {
            Configs = configs;
        }

        public abstract void Execute();
    }

    internal class ConnectRepositoryCommand : ConnectCommand
    {
        public ConnectRepositoryCommand(SystemConfigs configs) : base(configs)
        {
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    internal class ConnectDroneCommand : ConnectCommand
    {
        public ConnectDroneCommand(SystemConfigs configs) : base(configs)
        {
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
