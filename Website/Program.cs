using Common.Utils;
using WatsonWebserver;
using WatsonWebserver.Core;
internal class Program
{
    public static async Task Main()
    {
        ConsoleVariables cVars = new("cVars");

        string host = cVars.GetCVar<string>("web.host", ConsoleVariableType.String, "127.0.0.1");
        int port = cVars.GetCVar<int>("web.port", ConsoleVariableType.Int, 80);
        bool useCertificate = cVars.GetCVar<int>("web.ssl", ConsoleVariableType.Int, 0) == 1;

        WebserverSettings settings = new(host, port, useCertificate);


        Webserver server = new Webserver(settings, DefaultRoute);

        server.Start();

        Console.ReadKey();
    }

    static async Task DefaultRoute(HttpContextBase ctx)
    {
        await ctx.Response.Send("Mirhebah I work");
    }
}