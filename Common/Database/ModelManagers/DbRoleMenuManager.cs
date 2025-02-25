using Common.Database.Models;
using Common.Utils;
using Microsoft.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Common.Database.ModelManagers
{
    public static class DbRoleMenuManager
    {
        private static ConsoleVariables _cVars;
        public static void Init(ConsoleVariables cVars) => _cVars = cVars;

        public static async Task<DbRoleMenu?> GetRolemenu([NotNull] ulong guildSnowflake, ulong? messageSnowflake, string? menuName = null)
        {
            using DbConn? conn = await DbConn.Factory(_cVars);

            if (conn == null)
                return null;

            using SqlDataReader reader = await conn.ExecuteResultProcedure(Procedures.GetRoleMenu, 
                new Dictionary<string, object?>
                {
                    ["@guild_snowflake"] = guildSnowflake,
                    ["@message_snowflake"] = messageSnowflake,
                    ["@menu_name"] = menuName
                }
            );

            long id = reader.GetInt64(reader.GetOrdinal("id"));
            long guildId = reader.GetInt64(reader.GetOrdinal("guild_id"));
            ulong _messageSnowflake = (ulong)reader.GetInt64(reader.GetOrdinal("message_snowflake"));
            string _menuName = reader.GetString(reader.GetOrdinal("menu_name"));
            DateTime creationDate = reader.GetDateTime(reader.GetOrdinal("creation_date"));
            Stream menuData = reader.GetStream(reader.GetOrdinal("menu_data"));
            
            return new DbRoleMenu(id, guildId, _menuName, _messageSnowflake, creationDate, menuData);
        }

        public static async Task<bool> CreateRoleMenu(ulong guildSnowflake, ulong messageSnowflake, string menuName, byte[] data)
        {
            DbConn? conn = await DbConn.Factory(_cVars);

            if (conn == null)
                return false;

            return await conn.ExecuteNonResultProcedure(Procedures.CreateRoleMenu, new Dictionary<string, object>()
            {
                ["@guild_snowflake"] = guildSnowflake,
                ["@message_snowflake"] = messageSnowflake,
                ["@menu_name"] = menuName,
                ["@menu_data"] = data,
            }) == 1;
        }
        public static async Task<bool> UpdateRoleMenu(DbRoleMenu menu)
        {

        }
    }
}
