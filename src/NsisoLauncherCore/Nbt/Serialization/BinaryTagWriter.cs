using System;
using System.IO;
using System.Text;

namespace Cyotek.Data.Nbt.Serialization
{
  public class BinaryTagWriter : TagWriter
  {
    #region Constants

    private readonly TagState _state;

    private readonly Stream _stream;

    #endregion

    #region Constructors

    public BinaryTagWriter(Stream stream)
    {
      _state = new TagState(FileAccess.Write);
      _stream = stream;
    }

    #endregion

    #region Methods

    public override void Close()
    {
      _stream.Flush();
      _stream.Close();
    }

    public override void Flush()
    {
      _stream.Flush();
    }

    public override void WriteArrayValue(byte value)
    {
      this.WriteValue(value);
    }

    public override void WriteArrayValue(int value)
    {
      this.WriteValue(value);
    }

    public override void WriteArrayValue(long value)
    {
      this.WriteValue(value);
    }

    public override void WriteEndDocument()
    {
      _state.SetComplete();
    }

    public override void WriteEndTag()
    {
      _state.EndTag(this.WriteEnd);
    }

    public override void WriteStartArray(string name, TagType type, int count)
    {
      // ReSharper disable once ConvertIfStatementToSwitchStatement
      if (type == TagType.Byte)
      {
        type = TagType.ByteArray;
      }
      else if (type == TagType.Int)
      {
        type = TagType.IntArray;
      }
      else if (type == TagType.Long)
      {
        type = TagType.LongArray;
      }
      else if (type != TagType.ByteArray && type != TagType.IntArray && type != TagType.LongArray)
      {
        throw new ArgumentException("Only byte, 32bit integer or 64bit integer types are supported.", nameof(type));
      }

      this.WriteStartTag(name, type);
      this.WriteValue(count);
    }

    public override void WriteStartDocument()
    {
      _state.Start();
    }

    public override void WriteStartTag(string name, TagType type)
    {
      TagContainerState currentState;

      currentState = _state.StartTag(type);

      if (type != TagType.End && (currentState == null || currentState.ContainerType != TagType.List))
      {
        this.WriteValue((byte)type);
        this.WriteValue(name);
      }
    }

    public override void WriteStartTag(string name, TagType type, TagType listType, int count)
    {
      // HACK: This is messy, rethink

      this.WriteStartTag(name, type);

      _state.StartList(listType, count);

      _stream.WriteByte((byte)listType);
      this.WriteValue(count);
    }

    protected override void WriteValue(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        this.WriteValue((short)0);
      }
      else
      {
        byte[] buffer;

        buffer = Encoding.UTF8.GetBytes(value);

        if (buffer.Length > short.MaxValue)
        {
          throw new ArgumentException("String data would be truncated.");
        }

        this.WriteValue((short)buffer.Length);
        _stream.Write(buffer, 0, buffer.Length);
      }
    }

    protected override void WriteValue(short value)
    {
      byte[] buffer;

      buffer = BitConverter.GetBytes(value);

      if (IsLittleEndian)
      {
        BitHelper.SwapBytes(buffer, 0, BitHelper.ShortSize);
      }

      _stream.Write(buffer, 0, BitHelper.ShortSize);
    }

    protected override void WriteValue(long value)
    {
      byte[] buffer;

      buffer = BitConverter.GetBytes(value);

      if (IsLittleEndian)
      {
        BitHelper.SwapBytes(buffer, 0, BitHelper.LongSize);
      }

      _stream.Write(buffer, 0, BitHelper.LongSize);
    }

    protected override void WriteValue(int[] value)
    {
      if (value != null && value.Length != 0)
      {
        this.WriteValue(value.Length);
        foreach (int item in value)
        {
          this.WriteValue(item);
        }
      }
      else
      {
        this.WriteValue(0);
      }
    }

    protected override void WriteValue(long[] value)
    {
      if (value != null && value.Length != 0)
      {
        this.WriteValue(value.Length);
        foreach (long item in value)
        {
          this.WriteValue(item);
        }
      }
      else
      {
        this.WriteValue(0);
      }
    }

    protected override void WriteValue(int value)
    {
      byte[] buffer;

      buffer = BitConverter.GetBytes(value);

      if (IsLittleEndian)
      {
        BitHelper.SwapBytes(buffer, 0, BitHelper.IntSize);
      }

      _stream.Write(buffer, 0, BitHelper.IntSize);
    }

    protected override void WriteValue(float value)
    {
      byte[] buffer;

      buffer = BitConverter.GetBytes(value);

      if (IsLittleEndian)
      {
        BitHelper.SwapBytes(buffer, 0, BitHelper.FloatSize);
      }

      _stream.Write(buffer, 0, BitHelper.FloatSize);
    }

    protected override void WriteValue(double value)
    {
      byte[] buffer;

      buffer = BitConverter.GetBytes(value);

      if (IsLittleEndian)
      {
        BitHelper.SwapBytes(buffer, 0, BitHelper.DoubleSize);
      }

      _stream.Write(buffer, 0, BitHelper.DoubleSize);
    }

    protected override void WriteValue(byte value)
    {
      _stream.WriteByte(value);
    }

    protected override void WriteValue(byte[] value)
    {
      if (value != null && value.Length != 0)
      {
        this.WriteValue(value.Length);
        _stream.Write(value, 0, value.Length);
      }
      else
      {
        this.WriteValue(0);
      }
    }

    protected override void WriteValue(TagCollection value)
    {
      _state.StartList(value.LimitType, value.Count);

      _stream.WriteByte((byte)value.LimitType);

      this.WriteValue(value.Count);

      foreach (Tag item in value)
      {
        this.WriteTag(item);
      }
    }

    protected override void WriteValue(TagDictionary value)
    {
      foreach (Tag item in value)
      {
        this.WriteTag(item);
      }
    }

    private void WriteEnd()
    {
      _stream.WriteByte((byte)TagType.End);
    }

    #endregion
  }
}
