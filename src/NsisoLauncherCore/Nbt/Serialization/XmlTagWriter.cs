using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Cyotek.Data.Nbt.Serialization
{
  public sealed class XmlTagWriter : TagWriter
  {
    #region Constants

    private static readonly char[] _cDataTriggers =
    {
      '<',
      '>',
      '&'
    };

    private readonly TagState _state;

    private readonly XmlWriter _writer;

    #endregion

    #region Fields

    private StringBuilder _arraySb;

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor. <see cref="XmlWriter"/>
    /// </summary>
    /// <param name="writer">The writer.</param>
    public XmlTagWriter(XmlWriter writer)
    {
      _state = new TagState(FileAccess.Write);
      _writer = writer;
    }

    public XmlTagWriter(Stream stream)
    {
      XmlWriterSettings settings;

      settings = new XmlWriterSettings
      {
        Indent = true,
        Encoding = Encoding.UTF8
      };

      _state = new TagState(FileAccess.Write);
      _writer = XmlWriter.Create(stream, settings);
    }

    #endregion

    #region Methods

    public override void Close()
    {
      base.Close();

      _writer.Flush();
      _writer.Close();
    }

    public override void Flush()
    {
      _writer.Flush();
    }

    public override void WriteArrayValue(byte value)
    {
      if (_arraySb.Length != 0)
      {
        _arraySb.Append(' ');
      }

      _arraySb.Append(value);
    }

    public override void WriteArrayValue(int value)
    {
      if (_arraySb.Length != 0)
      {
        _arraySb.Append(' ');
      }

      _arraySb.Append(value);
    }

    public override void WriteArrayValue(long value)
    {
      if (_arraySb.Length != 0)
      {
        _arraySb.Append(' ');
      }

      _arraySb.Append(value);
    }

    public override void WriteEndDocument()
    {
      _state.SetComplete();

      _writer.WriteEndDocument();
      _writer.Flush();
    }

    public override void WriteEndTag()
    {
      TagType currentTag;

      currentTag = _state.CurrentTag;

      if ((currentTag == TagType.ByteArray || currentTag == TagType.IntArray || currentTag == TagType.LongArray) && _arraySb != null && _arraySb.Length != 0)
      {
        _writer.WriteValue(_arraySb.ToString());
        _arraySb.Length = 0;
      }

      _state.EndTag();

      _writer.WriteEndElement();
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
      else if (type != TagType.ByteArray && type != TagType.IntArray && type != TagType.Long)
      {
        throw new ArgumentException("Only byte, 32bit integer or 64bit integer types are supported.", nameof(type));
      }

      if (_arraySb == null)
      {
        _arraySb = new StringBuilder();
      }

      this.WriteStartTag(name, type);
    }

    public override void WriteStartDocument()
    {
      _state.Start();

      _writer.WriteStartDocument(true);
    }

    public override void WriteStartTag(string name, TagType type)
    {
      TagContainerState currentState;

      currentState = _state.StartTag(type);

      if (string.IsNullOrEmpty(name))
      {
        name = "tag";
      }

      if (XmlConvert.EncodeName(name) == name)
      {
        _writer.WriteStartElement(name);
      }
      else
      {
        _writer.WriteStartElement("tag");
        _writer.WriteAttributeString("name", name);
      }

      if (type != TagType.End && (currentState == null || currentState.ContainerType != TagType.List))
      {
        _writer.WriteAttributeString("type", type.ToString());
      }
    }

    public override void WriteStartTag(string name, TagType type, TagType listType, int count)
    {
      this.WriteStartTag(name, type);

      _state.StartList(listType, count);

      _writer.WriteAttributeString("limitType", listType.ToString());
    }

    protected override void WriteValue(string value)
    {
      if (value != null)
      {
        if (value.IndexOfAny(_cDataTriggers) != -1)
        {
          _writer.WriteCData(value);
        }
        else
        {
          _writer.WriteValue(value);
        }
      }
    }

    protected override void WriteValue(short value)
    {
      _writer.WriteValue(value);
    }

    protected override void WriteValue(long value)
    {
      _writer.WriteValue(value);
    }

    protected override void WriteValue(int[] value)
    {
      StringBuilder output;

      output = new StringBuilder();

      foreach (int i in value)
      {
        if (output.Length != 0)
        {
          output.Append(' ');
        }

        output.Append(i);
      }

      _writer.WriteValue(output.ToString());
    }

    protected override void WriteValue(long[] value)
    {
      StringBuilder output;

      output = new StringBuilder();

      foreach (long i in value)
      {
        if (output.Length != 0)
        {
          output.Append(' ');
        }

        output.Append(i);
      }

      _writer.WriteValue(output.ToString());
    }

    protected override void WriteValue(int value)
    {
      _writer.WriteValue(value);
    }

    protected override void WriteValue(float value)
    {
      _writer.WriteValue(value);
    }

    protected override void WriteValue(double value)
    {
      _writer.WriteValue(value);
    }

    protected override void WriteValue(byte value)
    {
      _writer.WriteValue(value);
    }

    protected override void WriteValue(byte[] value)
    {
      StringBuilder output;

      output = new StringBuilder();

      foreach (byte i in value)
      {
        if (output.Length != 0)
        {
          output.Append(' ');
        }

        output.Append(i);
      }

      _writer.WriteValue(output.ToString());
    }

    protected override void WriteValue(TagCollection value)
    {
      _state.StartList(value.LimitType, value.Count);

      _writer.WriteAttributeString("limitType", value.LimitType.ToString());

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

    #endregion
  }
}
