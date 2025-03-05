using Common.Database.ModelManagers;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Text.Json;

namespace Common.Database.Models
{
    public class DbSession : IAsyncDisposable
    {
        public long Id { get; init; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; init; }
        private Dictionary<string, string> _sessionData { get; init; }
        private bool _changed = false;

        public DbSession(long id, string token, Dictionary<string, string> sessionData, DateTime expirationDate)
        {
            Id = id;
            Token = token;
            _sessionData = sessionData;
            ExpirationDate = expirationDate;
        }
        public string Serialize() => JsonSerializer.Serialize(_sessionData, new JsonSerializerOptions { WriteIndented = true });
        
        public bool Get(string key, out string? value)
        {
            bool result = _sessionData.TryGetValue(key, out string? _value);

            value = _value;
            return result;
        }
        public void Set(string key, string value)
        {
            if (DateTime.UtcNow > ExpirationDate)
                throw new InvalidOperationException("Cannot modify expired session");

            _changed = true;
            _sessionData[key] = value;
        }
        public void Dispose() {
            if (_changed)
            {
                 Task<bool> task = DbSessionManager.Update(this);

                task.Wait();

                if (task.Result != true)
                    throw new HttpProtocolException(500, "Failed to update session", null);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if(_changed)
            {
                bool result = await DbSessionManager.Update(this);

                if (!result)
                    throw new Exception("Failed to update session");
            }
        }
    }
}
