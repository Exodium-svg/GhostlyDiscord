namespace Common.ConsoleCommands
{
    public sealed class ConsoleCommandHandler()
    {
        readonly Dictionary<string, IConsoleCommand> _commandsMap = new();
        public void Start()
        {
            while (true)
            {
                string? command = Console.ReadLine();

                if (command == null)
                    continue;

                if (command == "quit")
                    return;

                Span<string> commandInfo = command.Split(' ');

                CommandResult result = ExecuteCommand(commandInfo[0], commandInfo.Slice(1, commandInfo.Length - 1));

                switch(result)
                {
                    case CommandResult.Success:
                        Console.WriteLine($"{commandInfo[0]} executed successfully.");
                        break;
                    case CommandResult.Unknown:
                        Console.WriteLine($"{commandInfo[0]} is not a known command.");
                        break;
                }  
            }
        }
        public void RegisterCommand(IConsoleCommand command) => _commandsMap[command.GetName()] = command;
        public CommandResult ExecuteCommand(string command, scoped Span<string> args)
        {
            if (_commandsMap.TryGetValue(command, out IConsoleCommand? consoleCommand))
            {
                CommandResult result = consoleCommand.Execute(args);

                switch (result)
                {
                    case CommandResult.Success:
                        return result;
                    default:
                        Console.WriteLine($"Console error {consoleCommand.GetError()}");
                        return result;
                }
            }

            return CommandResult.Unknown;
        }
    }
}
