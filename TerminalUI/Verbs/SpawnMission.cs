using CommandLine;

namespace TerminalUI.Verbs
{
    [Verb("spawn-mission")]
    internal class SpawnMission : IVerb
    {
        [Value(0, HelpText = "Coordinata X del punto di partenza", Required = true)]
        public double Xstart { get; set; }

        [Value(1, HelpText = "Coordinata Y del punto di partenza", Required = true)]
        public double Ystart { get; set; }

        [Value(2, HelpText = "Coordinata X del punto di arrivo", Required = true)]
        public double Xend { get; set; }

        [Value(3, HelpText = "Coordinata Y del punto di arrivo", Required = true)]
        public double Yend { get; set; }

        [Option('h', HelpText = "Hostname dell'ActorSystem", Default = "localhost")]
        public string? Host { get; set; }

        [Option('p', HelpText = "Porta dell'ActorSystem su cui spawnare la missione", Required = true)]
        public int Port { get; set; }

        public Environment Run(Environment env)
        {
            throw new NotImplementedException();
        }
    }
}
