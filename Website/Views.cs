using Fluid;
using Fluid.Parser;

namespace Website
{
    public static class Views
    {
        private static FluidParser _parser = new();
        const string path = "public/templates/";

        public static async Task<string> GetErrorPage(int errorCode, string text)
        {
            return await Views.Get("error-page.html", new Dictionary<string, object>()
            {
                ["errorcode"] = errorCode,
                ["text"] = text
            });
        }
        public static async Task<string> Get(string viewName, Dictionary<string, object>? values = null)
        {
            string viewPath = Path.Combine(path, viewName);

            if (!File.Exists(viewPath))
                throw new Exception($"{viewName} does not exist");

            string template = await File.ReadAllTextAsync(viewPath);

            TemplateContext context = new TemplateContext();

            if(values is not null)
                foreach (KeyValuePair<string, object> value in values)
                    context.SetValue(value.Key, value.Value);

            IFluidTemplate fluidTemplate = _parser.Parse(template);
    
            string html = await fluidTemplate.RenderAsync(context);

            return html;
        }
    }
}
