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

        private string GetDiscordToken(DbSession session)
        {
            if (!session.Get("discord.token", out string? token))
                throw new HttpProtocolException(500, "Discord token does not exist", null);

            return token!;
        }
    }
}
