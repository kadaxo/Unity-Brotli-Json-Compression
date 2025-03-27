using System.IO;
using System.IO.Compression;
using System.Text;

public static class BrotliUtility
{
    // Compression with level support
    public static byte[] Compress(string input, CompressionLevel level = CompressionLevel.Optimal)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        using var output = new MemoryStream();
        using (var compressor = new BrotliStream(output, level))
        {
            compressor.Write(inputBytes, 0, inputBytes.Length);
        }
        return output.ToArray();
    }

    // Decompress from bytes
    public static string Decompress(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var output = new MemoryStream();
        using (var decompressor = new BrotliStream(input, CompressionMode.Decompress))
        {
            decompressor.CopyTo(output);
        }
        return Encoding.UTF8.GetString(output.ToArray());
    }

    // Decompress from Base64 string (for imported assets)
    public static string DecompressFromBase64(string base64String)
    {
        byte[] compressedData = System.Convert.FromBase64String(base64String);
        return Decompress(compressedData);
    }
}