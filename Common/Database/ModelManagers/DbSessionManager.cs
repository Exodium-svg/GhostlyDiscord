using Common.Database.Models;
using Common.Utils;

namespace Common.Database.ModelManagers
{
    public static class DbSessionManager
    {
        static ConsoleVariables _cVars;

        public static void Init(ConsoleVariables cVars) => _cVars = cVars;

        public static async Task<DbSession?> Get(string identifier)
        {
            // do query shit
            return null;
        }
        public static async Task<bool> Update(DbSession session)
        {
            return false;
        }
        public static async Task<string> Create()
        {
            return string.Empty;
        }
    }
}
