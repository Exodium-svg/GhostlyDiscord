using Discord;
using Discord.WebSocket;
using GhostlyDiscord;
using Common.Utils;
using GhostlyDiscord.xDiscord;
using Common.ConsoleCommands;
using GhostlyDiscord.ConsoleCommands.Commands;
using Common.Database.ModelManagers;
using Common.Database;
using Microsoft.Data.SqlClient;

internal class Progam
{
    public static async Task Main()
    {
        ConsoleVariables cVars = new("cVars");
        string token = cVars.GetCVar<string>("discord.token", ConsoleVariableType.String, "NULL");


        bool validDb = await ValidateDbConnection(cVars);

        if(!validDb)
            Environment.Exit(1);

        DiscordSocketConfig config = new DiscordSocketConfig();
        //config.TotalShards = int.Parse(args[0]);
        //config.ShardId = int.Parse(args[1]);
        // add configuration information for later.

        DiscordShardedClient client = new DiscordShardedClient(config);

        await Globals.Init(cVars, client);
        InitDbManagers(cVars);
        SetupEvents(client);

        ConsoleCommandHandler commandHandler = new ConsoleCommandHandler();

        RegisterCommands(commandHandler);


        await SetupClient(client, token, cVars);
        commandHandler.Start();

        using FileStream fStream = File.OpenWrite("cVars");
        cVars.SaveVariables(fStream);

        fStream.Close();
    }
    private static async Task<bool> ValidateDbConnection(ConsoleVariables cVars)
    {
        using DbConn? dbConn = await DbConn.Factory(cVars);

        if (dbConn == null)
        {
            Console.WriteLine("Db connection is invalid!");
            return false;
        }
        else
        {
            List<object[]>? data = await dbConn.ExecuteQuery("SELECT @@VERSION;");

            if (data == null)
            {
                Console.WriteLine("Db is not MSSQL?");
                return false;
            }

            Console.WriteLine(data[0][0] as string);
        }

        return true;
    }
    private static async Task SetupClient(DiscordShardedClient client, string token, ConsoleVariables cVars)
    {
        await client.LoginAsync(TokenType.Bot, token);

        if (client.LoginState != LoginState.LoggedIn)
            throw new Exception($"Invalid credentials? unable to login.");
        
        await client.StartAsync();
    }
    private static void InitDbManagers(ConsoleVariables cVars)
    {
        DbGuildManager.Init(cVars);
        DbRoleMenuManager.Init(cVars);
    }

    private static void RegisterCommands(ConsoleCommandHandler commandHandler)
    {
        commandHandler.RegisterCommand(new ConsoleSetCVar());

    }
    private static void SetupEvents(DiscordShardedClient client)
    {
        client.ReactionAdded += DiscordEvents.OnReactionAdded;
        client.UserJoined    += DiscordEvents.OnUserJoined;
        client.ShardReady    += DiscordEvents.OnShardReady;
        client.JoinedGuild   += DiscordEvents.OnJoinedGuild;
    }

    private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        throw new NotImplementedException();
    }
}