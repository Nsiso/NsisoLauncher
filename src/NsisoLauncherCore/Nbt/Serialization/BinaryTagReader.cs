using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Cyotek.Data.Nbt.Serialization
{
  public class BinaryTagReader : TagReader
  {
    #region Constants

    private readonly Stream _originalStream;

    private readonly TagState _state;

    private readonly Stream _stream;

    #endregion

    #region Constructors

    public BinaryTagReader(Stream stream)
      : this(stream, true)
    { }

    public BinaryTagReader(Stream stream, bool autoDetectCompression)
    {
      if (stream.CanSeek && autoDetectCompression)
      {
        if (stream.IsGzipCompressed())
        {
          _originalStream = stream;
          _stream = new GZipStream(_originalStream, CompressionMode.Decompress);
        }
        else if (stream.IsDeflateCompressed())
        {
          _originalStream = stream;
          _stream = new DeflateStream(_originalStream, CompressionMode.Decompress);
        }
        else
        {
          _stream = stream;
        }
      }
      else
      {
        _stream = stream;
      }

      _state = new TagState(FileAccess.Read);
      _state.Start();
    }

    #endregion

    #region Static Methods

    private static bool FirstDecompressedByteIsTagCompound(Stream stream)
    {
      bool result;

      try
      {
        using (Stream decompressionStream = new GZipStream(stream, CompressionMode.Decompress, true))
        {
          result = decompressionStream.ReadByte() == (int)TagType.Compound;
        }
      }
      catch (InvalidDataException)
      {
        result = false;
      }

      return result;
    }

    private static bool FirstDeflatedByteIsTagCompound(Stream stream)
    {
      bool result;

      try
      {
        using (Stream decompressionStream = new DeflateStream(stream, CompressionMode.Decompress, true))
        {
          result = decompressionStream.ReadByte() == (int)TagType.Compound;
        }
      }
      catch (InvalidDataException)
      {
        result = false;
      }

      return result;
    }

    #endregion

    #region Methods

    public override bool IsNbtDocument()
    {
      bool result;
      Stream stream;
      long position;

      stream = _originalStream ?? _stream;

      if (stream.CanSeek)
      {
        position = stream.Position;
      }
      else
      {
        position = -1;
      }

      if (stream.IsGzipCompressed())
      {
        result = FirstDecompressedByteIsTagCompound(stream);
      }
      else if (stream.IsDeflateCompressed())
      {
        result = FirstDeflatedByteIsTagCompound(stream);
      }
      else if (stream.ReadByte() == (int)TagType.Compound)
      {
        result = true;
      }
      else
      {
        result = false;
      }

      if (stream.CanSeek)
      {
        stream.Position = position;
      }

      return result;
    }

    public override byte ReadByte()
    {
      int data;

      data = _stream.ReadByte();

      if (data != (data & 0xFF))
      {
        throw new InvalidDataException();
      }

      return (byte)data;
    }

    public override byte[] ReadByteArray()
    {
      int length;
      byte[] data;

      length = this.ReadInt();
      data = new byte[length];

      if (length != _stream.Read(data, 0, length))
      {
        throw new InvalidDataException();
      }

      return data;
    }

    public override TagDictionary ReadCompound()
    {
      TagDictionary results;
      Tag tag;

      results = new TagDictionary();

      tag = this.ReadTag();

      while (tag.Type != TagType.End)
      {
        results.Add(tag);
        tag = this.ReadTag();
      }

      return results;
    }

    public override double ReadDouble()
    {
      byte[] data;

      data = new byte[BitHelper.DoubleSize];

      if (BitHelper.DoubleSize != _stream.Read(data, 0, BitHelper.DoubleSize))
      {
        throw new InvalidDataException();
      }

      if (TagWriter.IsLittleEndian)
      {
        BitHelper.SwapBytes(data, 0, BitHelper.DoubleSize);
      }

      return BitConverter.ToDouble(data, 0);
    }

    public override float ReadFloat()
    {
      byte[] data;

      data = new byte[BitHelper.FloatSize];

      if (BitHelper.FloatSize != _stream.Read(data, 0, BitHelper.FloatSize))
      {
        throw new InvalidDataException();
      }

      if (TagWriter.IsLittleEndian)
      {
        BitHelper.SwapBytes(data, 0, BitHelper.FloatSize);
      }

      return BitConverter.ToSingle(data, 0);
    }

    public override int ReadInt()
    {
      byte[] data;

      data = new byte[BitHelper.IntSize];

      if (BitHelper.IntSize != _stream.Read(data, 0, BitHelper.IntSize))
      {
        throw new InvalidDataException();
      }

      return BinaryTagReader.ReadInt(data,0);
    }

    private static int ReadInt(byte[] data, int offset)
    {
      if (TagWriter.IsLittleEndian)
      {
        BitHelper.SwapBytes(data, offset, BitHelper.IntSize);
      }

      return BitConverter.ToInt32(data, offset);
    }

    public override int[] ReadIntArray()
    {
      int length;
      int bufferLength;
      byte[] buffer;
      int[] values;
      bool isLittleEndian;

      isLittleEndian = TagWriter.IsLittleEndian;
      length = this.ReadInt();
      bufferLength = length * BitHelper.IntSize;
      buffer = new byte[bufferLength];

      if (bufferLength != _stream.Read(buffer, 0, bufferLength))
      {
        throw new InvalidDataException();
      }

      values = new int[length];

      for (int i = 0; i < length; i++)
      {
        values[i] = BinaryTagReader.ReadInt(buffer, i * BitHelper.IntSize);
      }

      return values;
    }
    public override long[] ReadLongArray()
    {
      int length;
      int bufferLength;
      byte[] buffer;
      long[] values;
      bool isLittleEndian;

      isLittleEndian = TagWriter.IsLittleEndian;
      length = this.ReadInt();
      bufferLength = length * BitHelper.LongSize;
      buffer = new byte[bufferLength];

      if (bufferLength != _stream.Read(buffer, 0, bufferLength))
      {
        throw new InvalidDataException();
      }

      values = new long[length];

      for (int i = 0; i < length; i++)
      {
        values[i] = BinaryTagReader.ReadLong(buffer, i * BitHelper.LongSize);
      }

      return values;
    }

    public override TagCollection ReadList()
    {
      TagCollection tags;
      int length;
      TagType listType;

      listType = (TagType)this.ReadByte();
      length = this.ReadInt();

      if (length > 0 && (listType < TagType.Byte || listType > TagType.LongArray))
      {
        throw new InvalidDataException($"Unexpected list type '{listType}' found.");
      }

      tags = new TagCollection(listType);

      for (int i = 0; i < length; i++)
      {
        Tag tag;

        tag = null;

        _state.StartTag(listType);

        switch (listType)
        {
          case TagType.Byte:
            tag = TagFactory.CreateTag(this.ReadByte());
            break;

          case TagType.ByteArray:
            tag = TagFactory.CreateTag(this.ReadByteArray());
            break;

          case TagType.Compound:
            tag = TagFactory.CreateTag(this.ReadCompound());
            break;

          case TagType.Double:
            tag = TagFactory.CreateTag(this.ReadDouble());
            break;

          case TagType.Float:
            tag = TagFactory.CreateTag(this.ReadFloat());
            break;

          case TagType.Int:
            tag = TagFactory.CreateTag(this.ReadInt());
            break;

          case TagType.IntArray:
            tag = TagFactory.CreateTag(this.ReadIntArray());
            break;

          case TagType.List:
            tag = TagFactory.CreateTag(this.ReadList());
            break;

          case TagType.Long:
            tag = TagFactory.CreateTag(this.ReadLong());
            break;

          case TagType.LongArray:
            tag = TagFactory.CreateTag(this.ReadLongArray());
            break;

          case TagType.Short:
            tag = TagFactory.CreateTag(this.ReadShort());
            break;

          case TagType.String:
            tag = TagFactory.CreateTag(this.ReadString());
            break;

            // Can never be hit due to the type check above
            //default:
            //  throw new InvalidDataException("Invalid list type.");
        }

        _state.EndTag();

        tags.Add(tag);
      }

      return tags;
    }

    public override long ReadLong()
    {
      byte[] data;

      data = new byte[BitHelper.LongSize];

      if (BitHelper.LongSize != _stream.Read(data, 0, BitHelper.LongSize))
      {
        throw new InvalidDataException();
      }

      return BinaryTagReader. ReadLong(data,0);
    }

    private static long ReadLong(byte[] data,int offset)
    {
      if (TagWriter.IsLittleEndian)
      {
        BitHelper.SwapBytes(data, offset, BitHelper.LongSize);
      }

      return BitConverter.ToInt64(data, offset);
    }

    public override short ReadShort()
    {
      byte[] data;

      data = new byte[BitHelper.ShortSize];

      if (BitHelper.ShortSize != _stream.Read(data, 0, BitHelper.ShortSize))
      {
        throw new InvalidDataException();
      }

      if (TagWriter.IsLittleEndian)
      {
        BitHelper.SwapBytes(data, 0, BitHelper.ShortSize);
      }

      return BitConverter.ToInt16(data, 0);
    }

    public override string ReadString()
    {
      short length;
      byte[] data;

      length = this.ReadShort();
      data = new byte[length];

      if (length != _stream.Read(data, 0, length))
      {
        throw new InvalidDataException();
      }

      return data.Length != 0 ? Encoding.UTF8.GetString(data) : null;
    }

    public override Tag ReadTag()
    {
      Tag result;
      TagType type;
      string name;
      TagContainerState state;

      type = this.ReadTagType();

      if (type > TagType.LongArray)
      {
        throw new InvalidDataException($"Unrecognized tag type: {type}.");
      }

      state = _state.StartTag(type);

      if (type != TagType.End && (state == null || state.ContainerType != TagType.List))
      {
        name = this.ReadTagName();
      }
      else
      {
        name = string.Empty;
      }

      result = null;

      switch (type)
      {
        case TagType.End:
          result = TagFactory.CreateTag(TagType.End);
          break;

        case TagType.Byte:
          result = TagFactory.CreateTag(name, this.ReadByte());
          break;

        case TagType.Short:
          result = TagFactory.CreateTag(name, this.ReadShort());
          break;

        case TagType.Int:
          result = TagFactory.CreateTag(name, this.ReadInt());
          break;

        case TagType.IntArray:
          result = TagFactory.CreateTag(name, this.ReadIntArray());
          break;

        case TagType.Long:
          result = TagFactory.CreateTag(name, this.ReadLong());
          break;

        case TagType.LongArray:
          result = TagFactory.CreateTag(name, this.ReadLongArray());
          break;

        case TagType.Float:
          result = TagFactory.CreateTag(name, this.ReadFloat());
          break;

        case TagType.Double:
          result = TagFactory.CreateTag(name, this.ReadDouble());
          break;

        case TagType.ByteArray:
          result = TagFactory.CreateTag(name, this.ReadByteArray());
          break;

        case TagType.String:
          result = TagFactory.CreateTag(name, this.ReadString());
          break;

        case TagType.List:
          result = TagFactory.CreateTag(name, this.ReadList());
          break;

        case TagType.Compound:
          result = TagFactory.CreateTag(name, this.ReadCompound());
          break;
      }

      _state.EndTag();

      return result;
    }

    public override string ReadTagName()
    {
      return this.ReadString();
    }

    public override TagType ReadTagType()
    {
      return (TagType)_stream.ReadByte();
    }

    #endregion
  }
}
