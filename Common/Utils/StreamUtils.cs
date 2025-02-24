using System.Runtime.InteropServices;
using System.Text;

namespace Common.Utils
{
    public static class StreamUtils
    {
        public static T Read<T>(this Stream stream) where T : struct {

            Span<byte> sBuffer = stackalloc byte[Marshal.SizeOf<T>()];
            stream.ReadExactly(sBuffer);

            return MemoryMarshal.Read<T>(sBuffer);
        }

        public static string ReadString(this Stream stream)
        {
            int strLen = stream.Read<int>();
            Span<byte> shBuffer = strLen > 1024 ? new byte[strLen] : stackalloc byte[strLen];

            stream.ReadExactly(shBuffer);
            return Encoding.UTF8.GetString(shBuffer);
        }

        public static void Write<T>(this Stream stream, T value) where T : struct
        {
            Span<byte> sBuffer = stackalloc byte[Marshal.SizeOf<T>()];
            MemoryMarshal.Write(sBuffer, in value);

            stream.Write(sBuffer);
        }

        public static void WriteString(this Stream stream, string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value);

            stream.Write<int>(buffer.Length);
            stream.Write(buffer);
        }
    }
}
