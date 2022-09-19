using UI.InputCommands;

namespace UI.TerminalCommands
{
    internal class InputCommand : ITerminalCommand
    {
        public void Execute()
        {
            string? stringCommand;
            while ((stringCommand = Console.ReadLine()) != "exit" && stringCommand is not null)
            {
                var parameters = stringCommand.Split(' ');
                ITerminalCommand command;
                try
                {
                    command = (parameters[0], parameters[1]) switch
                    {
                        ("spawn", "repository") => new SpawnDroneCommand(),
                        ("spawn", "drone") => new SpawnRepositoryCommand(),
                        ("cancel", _) => new CancelMissionCommand(),
                        (_, _) => new InvalidCommand()
                    };
                }
                catch (Exception)
                {
                    command = new InvalidCommand();
                }

                command.Execute();
            }

            return;
        }
    }
}
