

using Akka.Actor;
using Akka.Configuration;
using DroneSystemAPI;
using UI;
using UI.InputCommands;
using UI.TerminalCommands;
/**
* Modalità per il main:
* 1) Solo Terminale
* 2) Solo Actor System
*  2a) Drone
*  2b) Repository
*/
if (args.Length == 0)
{
	new InputCommand().Execute();
}
else if (args.Length >= 1)
{
	ITerminalCommand command;
	try
	{
		var allArgs = args.ToList();
		if (args.Length == 1)
			allArgs.Add(null);

		command = (allArgs[0], allArgs[1]) switch
		{
			//("-d", "-c") => new ConnectDroneCommand(SystemConfigs.DroneConfig),
			//("-r", "-c") => new ConnectRepositoryCommand(SystemConfigs.RepositoryConfig),
			("-d", null) => new ActorSystemCommand(SystemConfigs.DroneConfig),
			("-r", null) => new ActorSystemCommand(SystemConfigs.RepositoryConfig),
			(_, _) => new InvalidArgsCommand()
        };
	}
	catch (Exception)
	{
		command = new InvalidArgsCommand();
	}
	command.Execute();
}