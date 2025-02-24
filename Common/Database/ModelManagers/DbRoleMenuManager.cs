using Common.Utils;

namespace Common.Database.ModelManagers
{
    public static class DbRoleMenuManager
    {
        private static ConsoleVariables _cVars;
        public static void Init(ConsoleVariables cVars) => _cVars = cVars;
    }
}
