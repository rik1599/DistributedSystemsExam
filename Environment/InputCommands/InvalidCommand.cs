namespace UI.InputCommands
{
    internal class InvalidCommand : ITerminalCommand
    {
        public void Execute()
        {
            Console.Error.WriteLine("Errore! Comando non riconosciuto o argomenti insufficienti!");
        }
    }
}
