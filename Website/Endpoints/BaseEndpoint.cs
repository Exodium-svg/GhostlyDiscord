using Common.Database.ModelManagers;
using Common.Database.Models;
using Parlot.Fluent;
using WatsonWebserver.Core;

namespace Website.Endpoints
{
    public abstract class BaseEndpoint
    {
        protected string GetToken(HttpContextBase context) => context.Request.Headers.Get("token") ?? throw new HttpProtocolException(400, "Missing token", null);
        protected string GetQueryParameter(HttpContextBase context, string key)
        {
            string? parameter = context.Request.Query.Elements.Get(key);

            if (parameter == null)
                throw new HttpProtocolException(400, "Invalid query", null);

            return parameter;
        }

        protected string? GetQueryParameterNullable(HttpContextBase context, string key) => context.Request.Query.Elements.Get(key);

        protected async Task<DbSession> GetSession(HttpContextBase context)
        {
            string? token = GetToken(context);

            if (token == null)
                throw new HttpProtocolException(400, "No token found", null);

            DbSession? session = await DbSessionManager.Get(token);

            if (session == null)
                throw new Exception("Failed to fetch session from database");

            return session;
        }
    }
}
