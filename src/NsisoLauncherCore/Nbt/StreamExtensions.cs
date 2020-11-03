using System.IO;

namespace Cyotek.Data.Nbt
{
  internal static class StreamExtensions
  {
    #region Static Methods

    public static bool IsDeflateCompressed(this Stream stream)
    {
      int buffer;
      long position;
      bool result;

      // http://www.gzip.org/zlib/rfc-deflate.html#spec

      position = stream.Position;
      buffer = stream.ReadByte();
      result = buffer != -1;

      if (result)
      {
        bool bit1Set;
        bool bit2Set;
        bool bit3Set;
        byte header;

        header = (byte)buffer;

        bit1Set = (header & (1 << 0)) != 0;
        bit2Set = (header & (1 << 1)) != 0;
        bit3Set = (header & (1 << 2)) != 0;

        result = bit1Set && (bit2Set || bit3Set) && !(bit2Set && bit3Set);
      }

      stream.Position = position;

      return result;
    }

    public static bool IsGzipCompressed(this Stream stream)
    {
      int bytesRead;
      long position;
      byte[] buffer;
      bool result;

      // http://www.gzip.org/zlib/rfc-gzip.html#file-format

      position = stream.Position;

      buffer = new byte[4];

      bytesRead = stream.Read(buffer, 0, 4);
      result = bytesRead == 4;

      if (result)
      {
        result = buffer[0] == 31 && buffer[1] == 139 && buffer[2] == 8;
      }

      stream.Position = position;

      return result;
    }

    #endregion
  }
}
