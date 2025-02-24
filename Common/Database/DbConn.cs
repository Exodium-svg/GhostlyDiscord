using Common.Utils;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Common.Database
{
    public class DbConn : IDisposable
    {
        readonly SqlConnection _conn;

        public static DbConn? Factory(ConsoleVariables cVars)
        {
            string address = cVars.GetCVar("db.address", ConsoleVariableType.String, "127.0.0.1");
            string database = cVars.GetCVar("db.name", ConsoleVariableType.String, "DiscordSharded");
            string username = cVars.GetCVar("db.user", ConsoleVariableType.String, "sa");
            string password = cVars.GetCVar("db.password", ConsoleVariableType.String, "Redemption92554477");

            string connectionString = $"Server={address}; Database={database};Trusted_Connection=True; User Id={username}; Password={password};";

            try
            {
                return new DbConn(connectionString);
            }
            catch (Exception)
            {
                Console.WriteLine($"Failed to connect to database on address {address}");
                return null;
            }
            
        }
        internal DbConn(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
            _conn.Open();
        }

        public async Task<int> ExecuteNonResultProcedure(string procedure, Dictionary<string, object> values)
        {
            using SqlCommand command = _conn.CreateCommand();

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;

            foreach (KeyValuePair<string, object> kvp in values)
                command.Parameters.AddWithValue(kvp.Key, kvp.Value);

            return await command.ExecuteNonQueryAsync();
        }
        public async Task<SqlDataReader> ExecuteResultProcedure(string procedure, Dictionary<string, object> values)
        {
            using SqlCommand command = _conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;

            foreach (KeyValuePair<string, object> kvp in values)
                command.Parameters.AddWithValue(kvp.Key, kvp.Value);

            return await command.ExecuteReaderAsync();
        }
        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}
