using Common.Utils;

namespace Common.Database.Models
{
    public class DbRoleMenu
    {
        public long Id { get; init; } // can't change, primary key
        public long GuildId { get; init; } // can't change foreign key
        public string MenuName { get; set; }
        public ulong MessageSnowflake { get; set; }
        public DateTime CreationDate { get; init; } // can't change.
        public Dictionary<ulong, ulong> EmojiRoleMap { get; set; } // Also will be turned into menu data

        public DbRoleMenu(long id, long guildId, string menuName, ulong messageSnowflake, DateTime creationDate, in byte[] binaryData)
        {
            Id = id;
            GuildId = guildId;
            MenuName = menuName;
            MessageSnowflake = messageSnowflake;
            CreationDate = creationDate;
            EmojiRoleMap = new();

            using MemoryStream stream = new MemoryStream(binaryData);

            int count = stream.Read<int>();

            for(int i = 0; i < count; i++)
            {
                ulong emojiSnowflake = stream.Read<ulong>();
                ulong roleSnowflake = stream.Read<ulong>();

                EmojiRoleMap[emojiSnowflake] = roleSnowflake;
            }
        }

        public Span<byte> GetEmojiRoleBinary()
        {
            using MemoryStream stream = new MemoryStream();

            stream.Write<int>(EmojiRoleMap.Count);

            foreach(KeyValuePair<ulong, ulong> kvp in EmojiRoleMap)
            {
                stream.Write(kvp.Key);
                stream.Write(kvp.Value);
            }

            return stream.ToArray();
        }
    }
}
