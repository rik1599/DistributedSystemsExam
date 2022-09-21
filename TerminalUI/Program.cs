
using CommandLine;
using System.Reflection;
using TerminalUI.Verbs;
using Environment = TerminalUI.Environment;

var env = new Environment();

string? line;
while ((line = Console.ReadLine()) != "exit")
{
    if (line == string.Empty || line is null) continue;

    var commandArgs = line!.Split(' ');
    env = Parser.Default.ParseArguments(commandArgs, LoadVerbs())
        .MapResult(
           (IVerb options) => options.Run(env),
           errors => env
        ) ;
}

static Type[] LoadVerbs()
{
    return Assembly.GetCallingAssembly().GetTypes()
        .Where(t => t.GetCustomAttribute<VerbAttribute>() is not null).ToArray();
}