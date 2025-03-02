namespace Common.Database.Models
{
    public class DbSession
    {
        public long Id { get; init; }
        public string Token { get; set; }
        public Dictionary<string, object> SessionData { get; init; }
        public DateTime ExpirationDate { get; init; }

        public DbSession(long id, string token, Dictionary<string, object> sessionData, DateTime expirationDate)
        {
            Id = id;
            Token = token;
            SessionData = sessionData;
            ExpirationDate = expirationDate;
        }
    }
}
