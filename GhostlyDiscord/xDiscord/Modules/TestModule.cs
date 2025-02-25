using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace GhostlyDiscord.xDiscord.Modules
{
    public class TestModule : InteractionModuleBase<ShardedInteractionContext>
    {
        const ulong OWNER_ID = 215076444810903552;
#if DEBUG
        [SlashCommand("htmltest", "debugTest")]
        [RequireContext(ContextType.Guild)]
        public async Task HtmlTest()
        {
            if (Context.User.Id != OWNER_ID)
                return;

            string pfpUrl = this.Context.User.GetAvatarUrl();
            WelcomeImageGeneratorService service = new(File.ReadAllText("Resource/DefaultWelcome.html"));
            SocketTextChannel? channel = Context.Channel as SocketTextChannel;

            if(channel == null)
            {
                await RespondAsync("This command can only ran in GuildTextChannels");
                return;
            }
            
            using Stream stream = service.Generate(this.Context.User.Username, "test-debug", pfpUrl, 1024);
            await RespondWithFileAsync(stream, $"welcome-image-{Context.Guild.Name}-{Context.User.Username}.png");
        }
#endif
    }
}
