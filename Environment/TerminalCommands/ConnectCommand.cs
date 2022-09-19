using Akka.Actor;
using DroneSystemAPI;
using DroneSystemAPI.APIClasses;
using DroneSystemAPI.APIClasses.Repository;

namespace UI.TerminalCommands
{
    internal abstract class ConnectCommand : ITerminalCommand
    {
        protected SystemConfigs Configs { get; }
        protected Host Host { get; }

        public ConnectCommand(SystemConfigs configs, string host, int port)
        {
            Configs = configs;
            Host = new Host(host, port);
        }

        public abstract void Execute();
    }

    internal class ConnectRepositoryCommand : ConnectCommand
    {
        public ConnectRepositoryCommand(SystemConfigs configs, string host, int port) : base(configs, host, port)
        {
        }

        public override void Execute()
        {
            using var system = ActorSystem.Create(Configs.SystemName, Configs.Config);
            var repositoryAPI = new RepositoryProvider(system, Configs).SpawnRemote(Host);
            Console.ReadKey();
        }
    }

    internal class ConnectDroneCommand : ConnectCommand
    {
        public ConnectDroneCommand(SystemConfigs configs, string host, int port) : base(configs, host, port)
        {
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
