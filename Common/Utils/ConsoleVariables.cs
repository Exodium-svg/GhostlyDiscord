using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Common.Utils
{
    public class ConsoleVariables
    {
        private readonly ConcurrentDictionary<string, ConsoleVariable> _cVars = new();
        public int Count => _cVars.Count;
        public ConcurrentDictionary<string, ConsoleVariable> CVars => _cVars;
        public ConsoleVariables(string? path = null)
        {
            if (path == null)
                return;

            if (!File.Exists(path))
            {
                Console.WriteLine($"Cannot find ConsoleVariables at path: {path}");
                return;
            }

            using FileStream fs = File.OpenRead(path);
            LoadVariables(fs);
        }
        public T GetCVar<T>(string name, ConsoleVariableType type, T fallback)
        {
            if(_cVars.TryGetValue(name, out ConsoleVariable variable) && variable.Type == type && variable.Value is T castedValue)
                return castedValue;

            _cVars[name] = new ConsoleVariable { Name = name, Type = type, Value = fallback };

            return fallback;
        }
        public void SetCVar(string name, ConsoleVariableType type, object value) => _cVars[name] = new ConsoleVariable { Name = name, Type = type, Value = value };
        public void LoadVariables(Stream stream)
        {
            int varCount = stream.Read<int>();

            for(int i = 0; i < varCount; i++)
            {
                ConsoleVariable consoleVariable = new ConsoleVariable()
                {
                    Name = stream.ReadString(),
                    Type = (ConsoleVariableType)stream.Read<ushort>(),
                };
                
                consoleVariable.Value = consoleVariable.Type switch
                {
                    ConsoleVariableType.String => stream.ReadString(),
                    ConsoleVariableType.Int => stream.Read<int>(),
                    ConsoleVariableType.Float => stream.Read<float>(),
                    ConsoleVariableType.Int64 => stream.Read<long>(),
                    ConsoleVariableType.UInt64 => stream.Read<ulong>(),
                    _ => throw new NotSupportedException($"Variable type {(ushort)consoleVariable.Type} is not supported.")
                };

                _cVars[consoleVariable.Name] = consoleVariable;
            }
        }
        public void SaveVariables(Stream stream)
        {
            // Write the count of variables
            stream.Write(_cVars.Count);

            foreach (var kvp in _cVars)
            {
                ConsoleVariable consoleVariable = kvp.Value;

                stream.WriteString(consoleVariable.Name);
                stream.Write((ushort)consoleVariable.Type);

                switch (consoleVariable.Type)
                {
                    case ConsoleVariableType.String:
                        stream.WriteString((string)consoleVariable.Value);
                        break;
                    case ConsoleVariableType.Int:
                        stream.Write((int)consoleVariable.Value);
                        break;
                    case ConsoleVariableType.Float:
                        stream.Write((float)consoleVariable.Value);
                        break;
                    case ConsoleVariableType.Int64:
                        stream.Write((long)consoleVariable.Value);
                        break;
                    case ConsoleVariableType.UInt64:
                        stream.Write((ulong)consoleVariable.Value);
                        break;
                    default:
                        throw new NotSupportedException($"Variable type {(ushort)consoleVariable.Type} is not supported.");
                }
            }
        }
    }

    public enum ConsoleVariableType : ushort
    {
        String,
        Int,
        Float,
        Int64,
        UInt64
    }
    public struct ConsoleVariable
    {
        public string Name { get; init; }
        public ConsoleVariableType Type { get; init; }
        public object Value { get; set; }
    }
}
