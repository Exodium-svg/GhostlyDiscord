using Discord;
using Discord.WebSocket;
using GhostlyDiscord;
using Common.Utils;
using GhostlyDiscord.xDiscord;
using Common.ConsoleCommands;
using GhostlyDiscord.ConsoleCommands.Commands;
using Common.Files;
using Discord.Commands;
using System.Reflection;
using Discord.Interactions;

internal class Progam
{
    private static InteractionService commandService;
    public static async Task Main()
    {
        ConsoleVariables cVars = new("cVars");
        string token = cVars.GetCVar<string>("discord.token", ConsoleVariableType.String, "NULL");

        DiscordSocketConfig config = new DiscordSocketConfig();
        //config.TotalShards = int.Parse(args[0]);
        //config.ShardId = int.Parse(args[1]);
        // add configuration information for later.

        DiscordShardedClient client = new DiscordShardedClient(config);

        await Globals.Init(cVars, client);

        await client.LoginAsync(TokenType.Bot, token);

        if(client.LoginState != LoginState.LoggedIn)
            throw new Exception($"Invalid credentials? unable to login.");

        // setup shizzle for events.
        SetupEvents(client);
        await client.StartAsync();
        
        CommandHandler commandHandler = new CommandHandler();

        commandHandler.RegisterCommand(new ConsoleSetCVar());
        commandHandler.Start();

        using FileStream fStream = File.OpenWrite("cVars");
        cVars.SaveVariables(fStream);

        fStream.Close();
    }


    private static void SetupEvents(DiscordShardedClient client)
    {


        client.UserJoined  += DiscordEvents.OnUserJoined;
        client.ShardReady  += DiscordEvents.OnShardReady;
        client.JoinedGuild += DiscordEvents.OnJoinedGuild;
    }
}