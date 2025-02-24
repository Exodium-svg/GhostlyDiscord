using Common.ConsoleCommands;
using Common.Utils;
using System.Globalization;
namespace GhostlyDiscord.ConsoleCommands.Commands
{
    public class ConsoleSetCVar : IConsoleCommand
    {
        string _error = "none";
        public CommandResult Execute(scoped Span<string> parameters)
        {
            if (parameters.Length != 3)
            {
                _error = "Expected 3 parameters";
                return CommandResult.InvalidParameters;
            }

            string key = parameters[0];
            string varType = parameters[1];
            string value = parameters[2];

            if (!Enum.TryParse(varType, true, out ConsoleVariableType type))
            {
                _error = "Invalid varType";
                return CommandResult.Failed;
            }

            ConsoleVariables cVars = Globals.ConsoleVariables;

            switch (type)
            {
                case ConsoleVariableType.String:
                    cVars.SetCVar(key, type, value);
                    break;
                case ConsoleVariableType.Int:
                    if (!int.TryParse(value, out int intVal))
                    {
                        _error = "Invalid Int32";
                        return CommandResult.Failed;
                    }
                    cVars.SetCVar(key, type, intVal);
                    break;
                case ConsoleVariableType.Float:
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatVal))
                    {
                        _error = "Invalid float";
                        return CommandResult.Failed;
                    }
                    cVars.SetCVar(key, type, floatVal);
                    break;
                case ConsoleVariableType.Int64:
                    if (!long.TryParse(value, out long int64Val))
                    {
                        _error = "Invalid Int64";
                        return CommandResult.Failed;
                    }
                    cVars.SetCVar(key, type, int64Val);
                    break;
                case ConsoleVariableType.UInt64:
                    if (!ulong.TryParse(value, out ulong uint64Val))
                    {
                        _error = "Invalid UInt64";
                        return CommandResult.Failed;
                    }
                    cVars.SetCVar(key, type, uint64Val);
                    break;
                default:
                    _error = "Unsupported type";
                    return CommandResult.Failed;
            }

            return CommandResult.Success;
        }

        public string GetError() => _error;

        public string GetName() => "SetVar";
    }
}
