using Azure.Core;
using Common.Utils;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Website
{
    [Flags]
    public enum DiscordPermissions : ulong
    {
        CreateInstantInvite = 1UL << 0,      // 0x0000000000000001
        KickMembers = 1UL << 1,              // 0x0000000000000002
        BanMembers = 1UL << 2,               // 0x0000000000000004
        Administrator = 1UL << 3,            // 0x0000000000000008
        ManageChannels = 1UL << 4,           // 0x0000000000000010
        ManageGuild = 1UL << 5,              // 0x0000000000000020
        AddReactions = 1UL << 6,             // 0x0000000000000040
        ViewAuditLog = 1UL << 7,             // 0x0000000000000080
        PrioritySpeaker = 1UL << 8,          // 0x0000000000000100
        Stream = 1UL << 9,                   // 0x0000000000000200
        ViewChannel = 1UL << 10,             // 0x0000000000000400
        SendMessages = 1UL << 11,            // 0x0000000000000800
        SendTTSMessages = 1UL << 12,         // 0x0000000000001000
        ManageMessages = 1UL << 13,          // 0x0000000000002000
        EmbedLinks = 1UL << 14,              // 0x0000000000004000
        AttachFiles = 1UL << 15,             // 0x0000000000008000
        ReadMessageHistory = 1UL << 16,      // 0x0000000000010000
        MentionEveryone = 1UL << 17,         // 0x0000000000020000
        UseExternalEmojis = 1UL << 18,       // 0x0000000000040000
        ViewGuildInsights = 1UL << 19,       // 0x0000000000080000
        Connect = 1UL << 20,                 // 0x0000000000100000
        Speak = 1UL << 21,                   // 0x0000000000200000
        MuteMembers = 1UL << 22,             // 0x0000000000400000
        DeafenMembers = 1UL << 23,           // 0x0000000000800000
        MoveMembers = 1UL << 24,             // 0x0000000001000000
        UseVAD = 1UL << 25,                  // 0x0000000002000000
        ChangeNickname = 1UL << 26,          // 0x0000000004000000
        ManageNicknames = 1UL << 27,         // 0x0000000008000000
        ManageRoles = 1UL << 28,             // 0x0000000010000000
        ManageWebhooks = 1UL << 29,          // 0x0000000020000000
        ManageEmojisAndStickers = 1UL << 30, // 0x0000000040000000
        UseApplicationCommands = 1UL << 31,  // 0x0000000080000000
        RequestToSpeak = 1UL << 32,          // 0x0000000100000000
        ManageEvents = 1UL << 33,            // 0x0000000200000000
        ManageThreads = 1UL << 34,           // 0x0000000400000000
        CreatePublicThreads = 1UL << 35,     // 0x0000000800000000
        CreatePrivateThreads = 1UL << 36,    // 0x0000001000000000
        UseExternalStickers = 1UL << 37,     // 0x0000002000000000
        SendMessagesInThreads = 1UL << 38,   // 0x0000004000000000
        UseEmbeddedActivities = 1UL << 39,   // 0x0000008000000000
        ModerateMembers = 1UL << 40,         // 0x0000010000000000
    }
    public class DiscordRole
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Position { get; set; }
        public ulong Permissions { get; set; }
    }
    public static class DiscordApi
    {
        const string API_URL = "https://discord.com/api/v10";
        static ConsoleVariables _cVars;
        public static void Init(ConsoleVariables cVars) => _cVars = cVars;

        public static async Task<Dictionary<string, object?>> AuthenticateUser(string code)
        {
            using HttpClient httpClient = new();

            string clientId = _cVars.GetCVar("discord.client_id", ConsoleVariableType.String, "not known");
            string clientSecret = _cVars.GetCVar("discord.client_secret", ConsoleVariableType.String, "not known");
            string redirectUrl = _cVars.GetCVar("web.root", ConsoleVariableType.String, "http://www.localhost/discord/redirect");

            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", $"{redirectUrl}" }
            });

            using HttpResponseMessage response = await httpClient.PostAsync($"{API_URL}/oauth2/token", content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"OAuth token request failed: {response.StatusCode} {response.ReasonPhrase}. Response: {errorMessage}");
            }

            string data = await response.Content.ReadAsStringAsync();
            Dictionary<string, object?>? result = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (result == null)
                throw new HttpRequestException("OAuth response data was empty.");

            return result;
        }
        public static async Task<string> GetUserProfile(string token)
        {
            using HttpClient httpClient = new();

            // Set Authorization header with Bearer token
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response = await httpClient.GetAsync($"{API_URL}/users/@me");

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to fetch user profile: {response.StatusCode} {response.ReasonPhrase}. Response: {errorMessage}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<string> GetGuilds(string token)
        {
            using HttpClient httpClient = new();

            // Set Authorization header with Bearer token
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage response = await httpClient.GetAsync($"{API_URL}/users/@me/guilds");

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to fetch user profile: {response.StatusCode} {response.ReasonPhrase}. Response: {errorMessage}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        public static async Task<List<string>> GetUserRoles(string guildId, string userId, string accessToken)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.GetAsync($"${API_URL}/guilds/{guildId}/members/{userId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get user roles: {response.StatusCode}");

            string json = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            List<string> roles = root.GetProperty("roles").EnumerateArray()
                                     .Select(role => role.GetString()!)
                                     .ToList();

            return roles;
        }

        public static async Task<List<DiscordRole>> GetGuildRoles(string guildId, string accessToken)
        {
            string url = $"{API_URL}/guilds/{guildId}/roles";
            using HttpClient httpClient = new();

            using HttpRequestMessage request = new(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to fetch guild roles. Status: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DiscordRole>>(json) ?? new List<DiscordRole>();
        }
        public static async Task<bool> UserHasPermission(string guildId, string userId, DiscordPermissions permissions, string accessToken)
        {
            List<string> userRoles = await GetUserRoles(guildId, userId, accessToken);
            List<DiscordRole> guildRoles = await GetGuildRoles(guildId, accessToken);

            bool hasPermission = guildRoles.Where(role => userRoles.Contains(role.Id)).Where( role => (role.Permissions & (ulong)permissions) == 1).Any();

            return hasPermission;
        }

    }
}
