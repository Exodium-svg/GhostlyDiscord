using Common.Utils;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Website
{
    public static class DiscordApi
    {
        const string API_URL = "https://discord.com/api/v10";
        static ConsoleVariables _cVars;
        public static void Init(ConsoleVariables cVars) => _cVars = cVars;

        public static async Task<Dictionary<string,object?>> AuthenticateUser(string code)
        {
            using HttpClient httpClient = new();

            string token = _cVars.GetCVar("discord.token", ConsoleVariableType.String, "not known");
            string clientId = _cVars.GetCVar("discord.client_id", ConsoleVariableType.String, "not known");
            string redirectUrl = _cVars.GetCVar("web.root", ConsoleVariableType.String, "localhost");

            using HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "client_id", token },
                { "client_secret", clientId },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", $"{redirectUrl}/dashboard" },
                { "scope", "identify" }
            });

            using HttpResponseMessage response = await httpClient.PostAsync($"{API_URL}/oauth2/token", content);

            if(response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Invalid token");

            string data = await response.Content.ReadAsStringAsync();
            Dictionary<string, object?>? result = JsonSerializer.Deserialize<Dictionary<string, object?>>(data);

            if(result == null)
                throw new HttpProtocolException((long)response.StatusCode, "Result data was empty?", null);

            return result!;
        }
    }
}
