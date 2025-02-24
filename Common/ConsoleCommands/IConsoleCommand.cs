namespace Common.ConsoleCommands
{
    public enum CommandResult
    {
        Success,
        Failed,
        InvalidParameters,
        Unknown,
    }
    public interface IConsoleCommand
    {
        public string GetName();
        public string GetError();
        public CommandResult Execute(scoped Span<string> parameters);
    }
}
