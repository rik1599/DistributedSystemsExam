﻿using CommandLine;
using System.Reflection;
using TerminalUI.Verbs;
using Environment = TerminalUI.Environment;

Console.ResetColor();

_ = Parser.Default.ParseArguments(new string[1] {"help"}, LoadVerbs());
var env = new Environment();

string? line;
while ((line = Console.ReadLine()) != "exit")
{
    if (line == string.Empty || line is null) continue;

    if (line == "clear" || line == "cls")
    {
        Console.Clear();
        continue;
    }

    var commandArgs = line!.Split(' ');
    env = Parser.Default.ParseArguments(commandArgs, LoadVerbs())
        .MapResult(
           (IVerb options) => options.Run(env),
           errors => env
        );

    Console.ResetColor();
}
env.Terminate();

static Type[] LoadVerbs()
{
    return Assembly.GetCallingAssembly().GetTypes()
        .Where(t => t.GetCustomAttribute<VerbAttribute>() is not null).ToArray();
}