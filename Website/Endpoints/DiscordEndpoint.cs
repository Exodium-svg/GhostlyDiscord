using Common.Database.ModelManagers;
using Common.Database.Models;
using Common.Utils;
using System.Text.Json;
using WatsonWebserver.Core;

namespace Website.Endpoints
{
    public class DiscordEndpoint : BaseEndpoint
    {
        ConsoleVariables _cVars;
        public DiscordEndpoint(ConsoleVariables cVars)
        {
            _cVars = cVars;
        }

        public async Task Redirect(HttpContextBase context)
        {
            string code = GetQueryParameter(context, "code");

            await using DbSession session = await DbSessionManager.Create();

            Dictionary<string, object?> response = await DiscordApi.AuthenticateUser(code);

            if (!response.TryGetValue("access_token", out object? jsonParsedObj) && jsonParsedObj is JsonElement element && element.ValueKind == JsonValueKind.String)
                throw new Exception("Authentication request failed, access_token missing?");

            string accessToken = ((JsonElement)jsonParsedObj!).GetString()!;

            session.Set("discord.token", accessToken);

            string instructions = $"""
                <script>
                    localStorage.setItem("token", "{session.Token}");
                    window.location="/dashboard";
                </script>
                <html>
                    Redirecting
                </html>
            """;

            context.Response.StatusCode = 301;
            await context.Response.Send(instructions);
        }

        public async Task Profile(HttpContextBase context)
        {
            await using DbSession session = await GetSession(context);

            string jsonProfile = await DiscordApi.GetUserProfile(GetDiscordToken(session));
            await context.Response.Send(jsonProfile);
        }

        public async Task Guilds(HttpContextBase context)
        {
            await using DbSession session = await GetSession(context);

            string jsonGuilds = await DiscordApi.GetGuilds(GetDiscordToken(session));
            await context.Response.Send(jsonGuilds);
        }

        public async Task GetWelcomeSettings(HttpContextBase context)
        {
            if (!ulong.TryParse(GetQueryParameter(context, "guildId"), out ulong guildSnowflake))
                throw new HttpProtocolException(400, "Invalid snowflake", null);

            DbGuildSettings? settings = await DbGuildManager.GetGuildSettingsFromDb(guildSnowflake);

            if (settings == null)
                throw new HttpProtocolException(400, "Unknown snowflake", null);

            await context.Response.Send(JsonSerializer.Serialize<DbGuildSettings>(settings, new JsonSerializerOptions() { WriteIndented = true }));
        }

        public async Task UpdateWelcome(HttpContextBase context)
        {
            await using DbSession session = await GetSession(context);

            string guildId = GetQueryParameter(context, "guildId");
            bool useWelcome = GetQueryParameterNullable(context, "useWelcome") == "true";
            string? channelSnowflake = GetQueryParameterNullable(context, "welcomeChannel");

            ulong welcomeChannel = 0;

            if(channelSnowflake != null)
                if (!ulong.TryParse(GetQueryParameter(context, "welcomeChannel"), out welcomeChannel))
                    throw new HttpProtocolException(400, "Invalid channel ID is not a snowflake", null);

            if (!session.Get("discord.user.id", out string? userId))
                throw new Exception("UserID missing from session?");

            bool hasPermission = await DiscordApi.UserHasPermission(guildId, userId!, DiscordPermissions.ManageGuild, GetToken(context));

            if (!hasPermission)
                throw new HttpProtocolException(400, "Not allowed", null);

            DbGuildSettings? settings = await DbGuildManager.GetGuildSettingsFromDb(ulong.Parse(guildId));

            if (settings == null)
                throw new Exception("Failed to retrieve settings");

            settings.UseWelcome = useWelcome;

            if(channelSnowflake != null)
                settings.WelcomeChannel = welcomeChannel;

            await DbGuildManager.UpdateGuildSettings(settings);

            context.Response.StatusCode = 201;
        }
        private string GetDiscordToken(DbSession session)
        {
            if (!session.Get("discord.token", out string? token))
                throw new HttpProtocolException(500, "Discord token does not exist", null);

            return token!;
        }
    }
}
