namespace UI.TerminalCommands
{
    internal class InvalidArgsCommand : ITerminalCommand
    {
        public void Execute()
        {
            Console.Error.WriteLine("Errore! Comando non riconosciuto o argomenti insufficienti!");
        }
    }
}
