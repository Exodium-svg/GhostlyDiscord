namespace Common.Database.Models
{
    public struct DbGuild
    {
        public long Id { get; set; }
        public ulong Snowflake { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? SubscriptionDate { get; set; }

        public bool IsSubscribed() => SubscriptionDate != null && SubscriptionDate > DateTime.Now;

        public DbGuild(long id, ulong snowflake, DateTime creationDate, DateTime? subscriptionDate)
        {
            Id = id;
            Snowflake = snowflake;
            CreationDate = creationDate;
            SubscriptionDate = subscriptionDate;
        }
    }
}
