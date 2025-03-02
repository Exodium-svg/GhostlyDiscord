using Common.Database.ModelManagers;
using Common.Utils;
using System.Web;
using WatsonWebserver;
using WatsonWebserver.Core;
using Website;
internal class Program
{
    public static async Task Main()
    {
        ConsoleVariables cVars = new("cVars");

        string host = cVars.GetCVar<string>("web.host", ConsoleVariableType.String, "127.0.0.1");
        int port = cVars.GetCVar<int>("web.port", ConsoleVariableType.Int, 80);
        //int port = cVars.GetCVar<int>("web.port", ConsoleVariableType.Int, 26652);
        bool useCertificate = cVars.GetCVar<int>("web.ssl", ConsoleVariableType.Int, 0) == 1;

        WebserverSettings settings = new WebserverSettings(host, port, useCertificate);

        using Webserver server = new Webserver(settings, DefaultRoute);

        Globals.Init(cVars, server);
        server.Routes.PreAuthentication.Static.Add(WatsonWebserver.Core.HttpMethod.GET, "/dashboard/", DashboardRoute, MyExceptionRoute);
        server.Routes.PreAuthentication.Static.Add(WatsonWebserver.Core.HttpMethod.GET, "/discord/redirect", DiscordRedirect, MyExceptionRoute);
        server.Routes.PreAuthentication.Static.Add(WatsonWebserver.Core.HttpMethod.GET, "/up/", GetStatisticsRoute, MyExceptionRoute);

        server.Routes.PreRouting = PreRoute;
        await server.StartAsync();
        

        Console.ReadKey();
    }

    static async Task MyExceptionRoute(HttpContextBase ctx, Exception e)
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.Send(await Views.Get("error-page.html", new Dictionary<string, object>()
        {
            ["errorcode"] = 500,
            ["text"] = e.Message
        }));
    }
    static async Task DiscordRedirect(HttpContextBase ctx)
    {
        string? code = ctx.Request.Url.Parameters.Get("code");

        if(code == null)
        {
            await ctx.Response.Send(await Views.Get("error-page.html", new Dictionary<string, object>()
            {
                ["errorcode"] = 400,
                ["text"] = "invalid query"
            }));
            return;
        }

        Dictionary<string, object?> response = await DiscordApi.AuthenticateUser(code);

        string token = await DbSessionManager.Create();

        ctx.Response.StatusCode = 301;
        ctx.Response.Headers["token"] = token;
        ctx.Response.Headers["Location"] = $"/dashboard?code={response["access_token"]}";
        await ctx.Response.Send("Redirecting");
    }
    static async Task DashboardRoute(HttpContextBase ctx) => await ctx.Response.Send(await Views.Get("dashboard.html"));
    static async Task GetStatisticsRoute(HttpContextBase ctx)
    {
        WebserverStatistics statistics = Globals.WebServer.Statistics;
        string html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Web Server Statistics</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #121212;
            color: #fff;
            text-align: center;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            background: #1e1e1e;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0px 0px 15px rgba(255, 255, 255, 0.1);
        }}
        h1 {{
            font-size: 24px;
            color: #4CAF50;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }}
        th, td {{
            padding: 10px;
            text-align: left;
            border-bottom: 1px solid #444;
        }}
        th {{
            background: #333;
        }}
        tr:hover {{
            background: rgba(255, 255, 255, 0.1);
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Web Server Statistics</h1>
        <table>
            <tr><th>Metric</th><th>Value</th></tr>
            <tr><td>Start Time</td><td>{statistics.StartTime:yyyy-MM-dd HH:mm:ss}</td></tr>
            <tr><td>Up Time</td><td>{statistics.UpTime}</td></tr>
            <tr><td>Received Bytes</td><td>{statistics.ReceivedPayloadBytes:N0}</td></tr>
            <tr><td>Sent Bytes</td><td>{statistics.SentPayloadBytes:N0}</td></tr>
        </table>
    </div>
</body>
</html>";

        await ctx.Response.Send(html);
    }
    static async Task DefaultRoute(HttpContextBase ctx) => await ctx.Response.Send(await Views.Get("error-page.html", new Dictionary<string, object>()
    {
        ["errorcode"] = 404,
        ["text"] = "The page you are looking for might have been removed, had its name changed or is temporarily unavailable.",
    }));
    static async Task PreRoute(HttpContextBase ctx)
    {
        string requestPath = ctx.Request.Url.RawWithoutQuery.TrimStart('/'); // Remove leading '/'
        string[] path = requestPath.Split('/');

        if (path[0] == "favicon.ico")
        {
            // serve icon instead.
            if (File.Exists("public/favicon.ico"))
            {
                using FileStream icoStream = File.OpenRead("public/favicon.ico");
                ctx.Response.ContentType = GetContentType("public/favicon.ico");
                await ctx.Response.Send(icoStream.Length, icoStream);
            }

            return;
        }

        if (!requestPath.StartsWith("files/"))
            return;

        path[0] = "public";

        string decodedPath = HttpUtility.UrlDecode(requestPath); // Decode URL-encoded characters
        string filePath = Path.GetFullPath(Path.Combine(path));

        if (filePath == null || !File.Exists(filePath))
        {
            ctx.Response.StatusCode = 404;
            await ctx.Response.Send("File not found");
            return;
        }

        ctx.Response.ContentType = GetContentType(filePath);
        Console.WriteLine(filePath);
        using FileStream fs = File.OpenRead(filePath);
        await ctx.Response.Send(fs.Length, fs);
    }
    static string GetContentType(string filePath) =>
        Path.GetExtension(filePath).ToLower() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".gif" => "image/gif",
            ".json" => "application/json",
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
}