namespace Common.Files
{
    public class WebFile
    {
        public string Name { get; private set; }
        public string Path { get; init; }

        public ReadOnlyMemory<byte> Data { get; private set; }

        public WebFile(string name, string path, ReadOnlyMemory<byte> data)
        {
            Path = path;
            Name = name;
            Data = data;
        }

    }
}
