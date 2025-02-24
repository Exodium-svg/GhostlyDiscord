namespace Common.Database.Models
{
    public class DbGuildSettings
    {
        public long Id { get; set; }
        public long GuildId { get; set; }
        public bool UseWelcome { get; set; }
        public ulong? WelcomeChannel { get; set; }

        public DbGuildSettings(long id, long guildId, bool useWelcome, ulong? welcomeChannel)
        {
            Id = id;
            GuildId = guildId;
            UseWelcome = useWelcome;
            WelcomeChannel = welcomeChannel;
        }
    }
}
