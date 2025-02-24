namespace GhostlyWebStorage
{
    public class StorageRouter
    {
        string _path;
        public StorageRouter(string resourcePath)
        {
            _path = resourcePath;

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        public void CreateFile(string location, Stream inputStream)
        {
            string path = Path.Combine(_path, location);
            string? directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            Console.WriteLine($"Saved file at: {path}");
            inputStream.CopyTo(stream);
            stream.Close();
        }
        public Stream? GetFile(string location)
        {
            string path = Path.Combine(_path, location);
            if (!File.Exists(path))
                return null;

            Console.WriteLine($"Getting file from {path}");
            return File.OpenRead(path);
        }
    }
}
