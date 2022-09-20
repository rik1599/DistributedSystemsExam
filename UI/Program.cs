using CommandLine;
using UI.Verbs;

Parser.Default.ParseArguments<RepositoryOptions, DroneOptions, ConnectOptions>(args)
    .MapResult(
        (RepositoryOptions options) => RepositoryOptions.Run(options),
        (DroneOptions options) => DroneOptions.Run(options),
        (ConnectOptions options) => ConnectOptions.Run(options),
        errors => 1
    );