using HtmlAgilityPack;
using CoreHtmlToImage;
using System.Text;
namespace GhostlyDiscord.xDiscord
{
    public class WelcomeImageGeneratorService
    {
        string _html;

        public WelcomeImageGeneratorService(string html)
        {
            _html = html;
            //_html = File.ReadAllText("Resource/welcome.html");
        }

        public Stream Generate(string name, string text, string pictureUrl, int width = 1024)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(_html);

            HtmlNode? node = document.GetElementbyId("name");

            if (node == null)
                throw new Exception("Invalid html document for welcome image, name element not found");

            node.InnerHtml = name;
            node = document.GetElementbyId("text");

            if(node == null)
                throw new Exception("Invalid html document for welcome image, text element not found");

            node.InnerHtml = text;

            node = document.GetElementbyId("profile-picture");

            if(node == null)
                throw new Exception("Invalid html document for welcome image, profile-picture element not found");

            node.SetAttributeValue("src", pictureUrl);
            HtmlConverter converter = new();

            MemoryStream stream = new MemoryStream();
            document.Save(stream);
            
            byte[] data = converter.FromHtmlString(Encoding.UTF8.GetString(stream.ToArray()), width);
            stream.Position = 0;
            stream.SetLength(data.Length);
            stream.Write(data);

            return stream;
        }
    }
}
