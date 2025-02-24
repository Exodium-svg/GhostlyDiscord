using Discord.WebSocket;
using Common.Utils;
using Common.Files;
using Discord.Interactions;
using Discord.Commands;
using System.Reflection;
using GhostlyDiscord.xDiscord;
using Microsoft.Extensions.DependencyInjection;

namespace GhostlyDiscord
{
    public static class Globals
    {
        public static DiscordShardedClient DiscordShardedClient { get; private set; }
        public static InteractionService InteractionService { get; private set; }   
        public static ConsoleVariables ConsoleVariables { get; private set; }
        public static FileManager FileManager { get; private set; }
        // move this to some kind of guild manager?

        private static string? _cachedWebAddress = null;
        public static string WebServerAddress { get => _cachedWebAddress ?? ConsoleVariables.GetCVar("web.server.url", ConsoleVariableType.String, "Not known"); }
        public static async Task Init(ConsoleVariables cVars, DiscordShardedClient client)
        {
            ConsoleVariables = cVars;
            DiscordShardedClient = client;
            FileManager = new FileManager(cVars.GetCVar<string>("web.url", ConsoleVariableType.String, "http://localhost:6789/storage/"));
            InteractionService = new InteractionService(client);

            var services = new ServiceCollection().BuildServiceProvider();

            await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), services);


            client.InteractionCreated += DiscordEvents.OnInteractionCreated;

            client.Log += Client_Log;

        }

        private static async Task Client_Log(Discord.LogMessage arg)
        {
            Console.WriteLine(arg.Exception);
        }
    }
}
