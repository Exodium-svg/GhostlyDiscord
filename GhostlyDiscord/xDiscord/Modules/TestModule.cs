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

        [SlashCommand("rolemenutest", "debugTest")]
        [RequireContext(ContextType.Guild)]
        public async Task RoleMenuTest()
        {
            if (Context.User.Id != OWNER_ID)
                return;

            // Define the select menu options (Replace with actual role IDs & names)
            List<SelectMenuOptionBuilder> options = new()
        {
            new SelectMenuOptionBuilder()
                .WithLabel("Role 1")
                .WithValue("role_1"),
            new SelectMenuOptionBuilder()
                .WithLabel("Role 2")
                .WithValue("role_2"),
            new SelectMenuOptionBuilder()
                .WithLabel("Role 3")
                .WithValue("role_3")
        };

            // Create the select menu
            SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                .WithCustomId("role_menu")
                .WithPlaceholder("Select a role...")
                .WithMinValues(1)
                .WithMaxValues(1) // Allow selecting only one role
                .WithOptions(options);

            // Create the message component
            ComponentBuilder component = new ComponentBuilder()
                .WithSelectMenu(selectMenu);

            // Send the message with the select menu
            await RespondAsync("Choose a role from the menu:", components: component.Build());
        }
#endif
    }
}
