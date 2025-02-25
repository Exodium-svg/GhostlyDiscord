using Common.Database.ModelManagers;
using Common.Database.Models;
using Common.Files;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace GhostlyDiscord.xDiscord
{
    public static class DiscordEvents
    {
        public static async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachableMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction) {
            SocketTextChannel? textChannel = await channel.GetOrDownloadAsync() as SocketTextChannel;

            if (!reaction.User.IsSpecified) return;
            if (textChannel == null) return;
            if (textChannel is not SocketGuildChannel) return;

            IUserMessage message = await cachableMessage.GetOrDownloadAsync();

            ulong guildSnowflake = textChannel.Guild.Id;
            ulong messageSnowflake = message.Id;

            DbRoleMenu? menu = await DbRoleMenuManager.GetRolemenu(guildSnowflake, messageSnowflake, null);

            if (menu == null)
                return;

            if (!menu.EmojiRoleMap.TryGetValue(reaction.Emote.Name, out ulong roleSnowflake))
                return;

            SocketGuild guild = textChannel.Guild;
            SocketGuildUser? user = guild.GetUser(reaction.UserId);
            if(user == null)
            {
                Console.WriteLine("Failed to get user from guild.");
                return;
            }

            SocketRole? role = guild.GetRole(roleSnowflake);

            if(role == null)
            {
                await guild.Owner.SendMessageAsync($"Role menu {menu.MenuName} has an invalid role (deleted role?) this is a one time message letting you know it will be removed.");
                menu.EmojiRoleMap.Remove(reaction.Emote.Name);

                await DbRoleMenuManager.UpdateRoleMenu(menu);
                return;
            }

            await user.AddRoleAsync(roleSnowflake);
        }
        public static async Task OnInteractionCreated(SocketInteraction interaction)
        {
            ShardedInteractionContext context = new ShardedInteractionContext(Globals.DiscordShardedClient, interaction);

            await Globals.InteractionService.ExecuteCommandAsync(context, null);

            
        }
        public static async Task OnJoinedGuild(SocketGuild guild)
        {
            Console.WriteLine($"Added to new guild {guild.Name}");
        }
        public static async Task OnShardReady(DiscordSocketClient client)
        {
            Console.WriteLine($"client started on {client.CurrentUser.Username}");
#if DEBUG
            await Globals.InteractionService.RegisterCommandsToGuildAsync(1341508806781304938, false);
#else
            await Globals.InteractionService.RegisterCommandsGloballyAsync();
#endif
        }
        public static async Task OnUserJoined(SocketGuildUser guildUser)
        {
            // write something to get the guild settings.
            DbGuildSettings? settings = await DbGuildManager.GetGuildSettingsFromDb(guildUser.Guild.Id);

            if(settings == null)
            {
                Console.WriteLine($"Failed to retrieve guild for Joined event");
                return;
            }

            if (!settings.UseWelcome)
                return;
            
            SocketGuild socketGuild = guildUser.Guild;
            string html = string.Empty;
            WebFile? file = await Globals.FileManager.GetFileAsync(socketGuild.Id.ToString(), "welcome.html");

            if (file == null)
                try {
                    html = File.ReadAllText("Resource/DefaultWelcome.html");
                } catch(FileNotFoundException e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            else
                html = Encoding.UTF8.GetString(file.Data.Span);

            WelcomeImageGeneratorService welcomeService = new WelcomeImageGeneratorService(html);

            if (settings.WelcomeChannel == null)
            {
                Console.WriteLine($"!!!!Guild has a welcome module enabled without Welcome channel? data integrity in question!!!!");
                return;
            }

            SocketTextChannel? channel = guildUser.Guild.GetTextChannel(settings.WelcomeChannel!.Value);

            if(channel == null)
            {
                Console.WriteLine($"Unable to find welcome channel");
                return;
            }

            try
            {
                using Stream stream = welcomeService.Generate(guildUser.DisplayName, "Welcome to the server", guildUser.GetAvatarUrl());
                await channel.SendFileAsync(stream, $"welcome-image-{guildUser.Guild.Name}-{guildUser.DisplayName}.png");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}
