namespace UI.InputCommands
{
    internal abstract class SpawnCommand : ITerminalCommand
    {
        public abstract void Execute();
    }

    internal class SpawnRepositoryCommand : SpawnCommand
    {
        public override void Execute()
        {
            // Spawn nuovo terminale
        }
    }

    internal class SpawnDroneCommand : SpawnCommand
    {
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
