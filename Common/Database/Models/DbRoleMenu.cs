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
        public Dictionary<string, ulong> EmojiRoleMap { get; set; } // Also will be turned into menu data

        public DbRoleMenu(long id, long guildId, string menuName, ulong messageSnowflake, DateTime creationDate, Stream stream)
        {
            Id = id;
            GuildId = guildId;
            MenuName = menuName;
            MessageSnowflake = messageSnowflake;
            CreationDate = creationDate;
            EmojiRoleMap = new();

            int count = stream.Read<int>();

            for(int i = 0; i < count; i++)
            {
                string emojiName = stream.ReadString();
                ulong roleSnowflake = stream.Read<ulong>();

                EmojiRoleMap[emojiName] = roleSnowflake;
            }
        }

        public Span<byte> GetEmojiRoleBinary()
        {
            using MemoryStream stream = new MemoryStream();

            stream.Write<int>(EmojiRoleMap.Count);

            foreach(KeyValuePair<string, ulong> kvp in EmojiRoleMap)
            {
                stream.WriteString(kvp.Key);
                stream.Write(kvp.Value);
            }

            return stream.ToArray();
        }
    }
}
