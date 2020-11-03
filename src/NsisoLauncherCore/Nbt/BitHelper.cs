using System.Diagnostics;

namespace Cyotek.Data.Nbt
{
  internal static class BitHelper
  {
    #region Constants

    public const int DoubleSize = 8;

    public const int FloatSize = 4;

    public const int IntSize = 4;

    public const int LongSize = 8;

    public const int ShortSize = 2;

    #endregion

    #region Static Methods

    public static void SwapBytes(byte[] buffer, int offset, int length)
    {
#if DEBUG
      Debug.Assert(offset + length <= buffer.Length, "offset + length is larger than buffer");
#endif

      if (length > 1)
      {
        byte temp;
        int newIndex;

        if (length == 2)
        {
          temp = buffer[offset];
          buffer[offset] = buffer[offset + 1];
          buffer[offset + 1] = temp;
        }
        else if (offset == 0)
        {
          for (int index = (length + 1) / 2 - 1; index >= 0; index--)
          {
            newIndex = length - index - 1;
            temp = buffer[index];
            buffer[index] = buffer[newIndex];
            buffer[newIndex] = temp;
          }
        }
        else
        {
          for (int index = (length + 1) / 2 - 1; index >= 0; index--)
          {
            newIndex = length - index - 1;
            temp = buffer[offset + index];
            buffer[offset + index] = buffer[offset + newIndex];
            buffer[offset + newIndex] = temp;
          }
        }
      }
    }

    #endregion
  }
}
