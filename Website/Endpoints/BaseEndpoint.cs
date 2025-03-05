using Common.Database.ModelManagers;
using Common.Database.Models;
using Parlot.Fluent;
using WatsonWebserver.Core;

namespace Website.Endpoints
{
    public abstract class BaseEndpoint
    {
        //List<Func<HttpContextBase,Task>> 
        protected string? GetToken(HttpContextBase context) => context.Request.Headers.Get("token");
        protected string GetQueryParameter(HttpContextBase context, string key)
        {
            string? parameter = context.Request.Query.Elements.Get(key);

            if (parameter == null)
                throw new HttpProtocolException(400, "Invalid query", null);

            return parameter;
        }
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
