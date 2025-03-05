using Common.ConsoleCommands;
using Common.Database.ModelManagers;
using Common.Utils;
using GhostlyWebStorage;
using System.Diagnostics;
using System.Net;
using System.Text;

internal class Program
{
    public static void Main()
    {
        ConsoleVariables cVars = new("cVars");
        Thread.CurrentThread.Name = "Main/Command";

        Thread webThread = new Thread(() => WebLoop(cVars));
        webThread.Name = "Main/Web";
        webThread.Start();

        ConsoleCommandHandler commandHandler = new ConsoleCommandHandler();

        commandHandler.Start();
    }

    public static async void WebLoop(ConsoleVariables cVars)
    {
        HttpListener listener = new();

        int port = cVars.GetCVar("website.port", ConsoleVariableType.Int, 80);
        string url = cVars.GetCVar("website.url", ConsoleVariableType.String, "51.15.15.33");

        string webPrefix = $"http://{url}:{port}";
        listener.Prefixes.Add($"{webPrefix}/storage/");
        listener.Prefixes.Add($"{webPrefix}/file-upload/");

        listener.Start();

        Console.WriteLine($"Listening on address {webPrefix}.");

        StorageRouter storageRouter = new("public");
        while(true)
        {
            HttpListenerContext context = await listener.GetContextAsync();

            string? rawUrl = context.Request.RawUrl;
            string method = context.Request.HttpMethod;

            if (rawUrl == null)
            {
                context.Response.StatusCode = 400;
                context.Response.OutputStream.Close();
                return;
            }

            using Stream stream = context.Response.OutputStream;

            if (rawUrl.StartsWith("/storage"))
            {
                context.Response.ContentType = "application/octet-stream";


                string[] urlParts = rawUrl.Split('/');
                //TODO instead of using Get params we should use Url parts.
                bool valid = true;
                if(urlParts.Length != 4)
                    valid = false;
                
                string guildIdString = urlParts[2];
                string filename = urlParts[3];
                
                if (!valid)
                {
                    context.Response.StatusCode = 400;
                    stream.WriteString("Invalid request.");
                }
                else  // only accept GET and PUT.
                {
                    if (method == "PUT")
                    {
                        storageRouter.CreateFile($"{guildIdString}/{filename}", context.Request.InputStream);
                        context.Response.StatusCode = 201;
                    }
                    else if (method == "GET")
                    {
                        Stream? fGetStream = storageRouter.GetFile($"{guildIdString}/{filename}"); // One way I will suffer from this.

                        if (fGetStream == null)
                        {
                            context.Response.StatusCode = 404;
                            continue;
                        }
                        try
                        {
                            fGetStream.CopyTo(stream);
                        }
                        catch (Exception) { }
                    }
                }
            }
            // pretend it doesn't exist until it annoys me
            else if (rawUrl.StartsWith("/file-upload")) // TODO implement me
            {
                // needs some kind of thingy
                if (method == "GET")
                {
                    stream.Write(Encoding.UTF8.GetBytes("""
                        <!DOCTYPE html>
                        <html lang="en">
                        <head>
                            <meta charset="UTF-8">
                            <meta name="viewport" content="width=device-width, initial-scale=1.0">
                            <title>File Upload</title>
                        </head>
                        <body>
                            <form action="./file-upload" method="POST" enctype="multipart/form-data">
                                <input type="file" name="file-uploader">
                                <button type="submit" onclick="alert('uploading');">Upload</button>
                            </form>
                        </body>
                        </html>
                        
                        """));
                }
            }
            else if (method == "POST")
            {
                if (!context.Request.ContentType?.StartsWith("multipart/form-data") ?? true)
                {
                    context.Response.StatusCode = 400;
                    using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                    writer.Write("Invalid content type.");
                    return;
                }

                string boundary = context.Request.ContentType.Split("boundary=")[1];
                string uploadDir = "uploads";
                Directory.CreateDirectory(uploadDir);

                using StreamReader reader = new StreamReader(context.Request.InputStream);
                string line;
                string fileName = "";

                // implement this bs
            }
            else
            {
                byte[] wtfBuffer = Encoding.UTF8.GetBytes("How did you get here wtf?");
                context.Response.StatusCode = 404;
                stream.Write(wtfBuffer, 0, wtfBuffer.Length);
            }

            stream.Close();
            
        }
    } 
}