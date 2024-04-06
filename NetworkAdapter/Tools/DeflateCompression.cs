using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkAdapter.Tools
{
    public interface ICompression
    {
        public byte[] Compress(byte[] inpData);
        public byte[] Decompress(byte[] inpData);
    }

    public class DeflateCompression: ICompression
    {
        public byte[] Compress(byte[] inpData)
        {
            using var inputStream = new MemoryStream(inpData);
            using var compressStream = new MemoryStream();
            using var compressor = new DeflateStream(compressStream, CompressionMode.Compress);
            inputStream.CopyTo(compressor);
            compressor.Close();
            return compressStream.ToArray();
        }

        public byte[] Decompress(byte[] inpData)
        {
            using var inputStream = new MemoryStream(inpData);
            using var decompressStream = new MemoryStream();
            using var decompressor = new DeflateStream(inputStream, CompressionMode.Decompress);
            decompressor.CopyTo(decompressStream);
            return decompressStream.ToArray();

        }
    }
}
