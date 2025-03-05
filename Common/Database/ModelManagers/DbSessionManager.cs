using Common.Database.Models;
using Common.Utils;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace Common.Database.ModelManagers
{
    public static class DbSessionManager
    {
        static ConsoleVariables _cVars;

        public static void Init(ConsoleVariables cVars) => _cVars = cVars;

        public static async Task<DbSession?> Get(string identifier)
        {
            using DbConn? conn = await DbConn.Factory(_cVars);

            if (null == conn)
                return null;

            using SqlDataReader reader = await conn.ExecuteResultProcedure(Procedures.GetSession, new Dictionary<string, object?>()
            {
                ["@SessionToken"] = identifier
            });

            if (!await reader.ReadAsync())
                return null;

            long id = reader.GetInt64(reader.GetOrdinal("id"));
            string sessionToken = reader.GetString(reader.GetOrdinal("session_key"));
            DateTime expirationDate = reader.GetDateTime(reader.GetOrdinal("valid_date"));
            string? sessionJson = reader.GetString(reader.GetOrdinal("session_data"));

            Dictionary<string, string>? sessionData;

            if (sessionJson != null)
                sessionData = JsonSerializer.Deserialize<Dictionary<string, string>>(sessionJson) ?? new();
            else
                sessionData = new();

            return new DbSession(id, sessionToken, sessionData, expirationDate);
        }
        public static async Task<bool> Update(DbSession session)
        {
            using DbConn? dbConn = await DbConn.Factory(_cVars);

            if(null == dbConn) return false;

            using MemoryStream stream = new MemoryStream();

            int result = await dbConn.ExecuteNonResultProcedure(Procedures.UpdateSession, new Dictionary<string, object>
            {
                ["@id"] = session.Id,
                ["@data"] = session.Serialize(),
            });

            if (result > 0) return false;

            return true;
        }
        public static async Task<DbSession> Create()
        {
            using DbConn? dbConn = await DbConn.Factory(_cVars);
            if (dbConn == null)
                throw new Exception("Failed to connect to database");

            Dictionary<string, object?> output = await dbConn.ExecuteOutParamProcedure(Procedures.CreateSession, new Dictionary<string, SqlDbType>()
            {
                ["@Id"] = SqlDbType.BigInt,
                ["@SessionKey"] = SqlDbType.NVarChar,
                ["@ExpirationDate"] = SqlDbType.DateTime
            });

            if (!output.TryGetValue("@Id", out object? idVal) || idVal is not long id)
                throw new Exception("Procedure failed value ID not found");

            if (!output.TryGetValue("@SessionKey", out object? keyVal) || keyVal is not string sessionKey)
                throw new Exception("Procedure failed value SessionIdentifier not found");

            if (!output.TryGetValue("@ExpirationDate", out object? dateVal) || dateVal is not DateTime expirationDate)
                throw new Exception("Procedure failed value ExpirationDate not found");

            return new DbSession(id, sessionKey, new Dictionary<string, string>(), expirationDate);
        }
    }
}
