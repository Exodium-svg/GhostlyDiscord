using Common.Database.Models;
using Common.Utils;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Common.Database.ModelManagers
{
    public static class DbGuildManager // some kind of caching? keep in mind that it will not be in sync with the Database
    {
        private static ConsoleVariables _cVars;
        public static void Init(ConsoleVariables cVars)
        {
            _cVars = cVars;
        }
        public static async Task<DbGuild?> GetGuildFromDb(ulong guildSnowflake)
        {
            using DbConn? conn = await DbConn.Factory(_cVars);

            if (conn == null) return null;

            using SqlDataReader reader = await conn.ExecuteResultProcedure(Procedures.GetGuild, new Dictionary<string, object?>()
            {
                ["@guild_id"] = guildSnowflake
            });

            await reader.ReadAsync();

            long id = reader.GetInt64("id");
            ulong snowflake = (ulong)reader.GetInt64("snow_flake");
            DateTime creationDate = reader.GetDateTime("creation_date");
            DateTime? subscriptionDate = reader.GetDateTime("subscription_date");

            return new DbGuild(id, snowflake, creationDate, subscriptionDate);
        }
        public static async Task<DbGuildSettings?> GetGuildSettingsFromDb(ulong guildSnowflake)
        {
            using DbConn? conn = await DbConn.Factory(_cVars);

            if (conn == null) return null;

            using SqlDataReader reader = await conn.ExecuteResultProcedure(Procedures.GetGuildSetting, new Dictionary<string, object?>()
            {
                ["@guild_id"] = guildSnowflake,
            });

            await reader.ReadAsync();

            long id = reader.GetInt64("id");
            long guildId = reader.GetInt64("guild_id");
            bool useWelcome = reader.GetInt16("use_welcome") == 1;
            ulong? welcomeChannel = null;

            if (useWelcome)
                welcomeChannel = (ulong)reader.GetInt64("welcome_channel");

            return new DbGuildSettings(id, guildId, useWelcome, welcomeChannel);
        }
    }
}
