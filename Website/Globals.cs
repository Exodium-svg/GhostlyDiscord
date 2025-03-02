using Common.Utils;
using WatsonWebserver;

namespace Website
{
    public static class Globals
    {
        public static ConsoleVariables ConsoleVariables { get; private set; }
        public static Webserver WebServer { get; private set; }
        public static void Init(ConsoleVariables cVars, Webserver server)
        {
            ConsoleVariables = cVars;
            WebServer = server;
        }
    }
}
