using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Cyotek.Data.Nbt.Serialization;

namespace Cyotek.Data.Nbt
{
  public class NbtDocument
  {
    #region Fields

    private TagCompound _documentRoot;

    private string _fileName;

    private NbtFormat _format;

    #endregion

    #region Constructors

    public NbtDocument()
    {
      _format = NbtFormat.Binary;
      _documentRoot = (TagCompound)TagFactory.CreateTag(TagType.Compound);
    }

    public NbtDocument(TagCompound document)
      : this()
    {
      if (document == null)
      {
        throw new ArgumentNullException(nameof(document));
      }

      _documentRoot = document;
    }

    #endregion

    #region Static Methods

    public static NbtFormat GetDocumentFormat(string fileName)
    {
      NbtFormat format;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException("Cannot find file.", fileName);
      }

      using (Stream stream = File.OpenRead(fileName))
      {
        format = GetDocumentFormat(stream);
      }

      return format;
    }

    public static NbtFormat GetDocumentFormat(Stream stream)
    {
      NbtFormat format;
      long position;

      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      if (!stream.CanSeek)
      {
        throw new ArgumentException("Stream is not seekable.", nameof(stream));
      }

      position = stream.Position;

      try
      {
        if (new BinaryTagReader(stream).IsNbtDocument())
        {
          format = NbtFormat.Binary;
        }
        else
        {
          stream.Position = position;

          format = new XmlTagReader(stream).IsNbtDocument()
            ? NbtFormat.Xml
            : NbtFormat.Unknown;
        }
      }
      catch
      {
        format = NbtFormat.Unknown;
      }

      stream.Position = position;

      return format;
    }

    public static string GetDocumentName(string fileName)
    {
      string result;

      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException("Cannot find file.", fileName);
      }

      using (Stream stream = File.OpenRead(fileName))
      {
        result = GetDocumentName(new BinaryTagReader(stream));

        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (result == null)
        {
          stream.Seek(0, SeekOrigin.Begin);
          result = GetDocumentName(new XmlTagReader(stream));
        }
      }

      return result;
    }

    public static bool IsNbtDocument(string fileName)
    {
      return GetDocumentFormat(fileName) != NbtFormat.Unknown;
    }

    public static bool IsNbtDocument(Stream stream)
    {
      return GetDocumentFormat(stream) != NbtFormat.Unknown;
    }

    public static NbtDocument LoadDocument(string fileName)
    {
      NbtDocument document;

      document = new NbtDocument();
      document.Load(fileName);

      return document;
    }

    public static NbtDocument LoadDocument(Stream stream)
    {
      NbtDocument document;

      document = new NbtDocument();
      document.Load(stream);

      return document;
    }

    private static string GetDocumentName(TagReader reader)
    {
      string result;

      if (reader.IsNbtDocument())
      {
        reader.ReadTagType(); // advance the reader
        result = reader.ReadTagName();
      }
      else
      {
        result = null;
      }

      return result;
    }

    private static TagReader GetReader(NbtFormat format, Stream stream)
    {
      TagReader reader;

      switch (format)
      {
        case NbtFormat.Binary:
          reader = new BinaryTagReader(stream);
          break;
        case NbtFormat.Xml:
          reader = new XmlTagReader(stream);
          break;
        default:
          throw new InvalidDataException("Unrecognized or unsupported file format.");
      }

      return reader;
    }

    #endregion

    #region Properties

    public TagCompound DocumentRoot
    {
      get { return _documentRoot; }
    }

    public string FileName
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    public NbtFormat Format
    {
      get { return _format; }
      set
      {
        if (_format != value)
        {
          switch (value)
          {
            case NbtFormat.Binary:
            case NbtFormat.Xml:
              _format = value;
              break;
            default:
              throw new ArgumentOutOfRangeException(nameof(value), value, null);
          }
        }
      }
    }

    #endregion

    #region Methods

    public void Load()
    {
      this.Load(_fileName);
    }

    public void Load(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      if (!File.Exists(fileName))
      {
        throw new FileNotFoundException("File not found.", fileName);
      }

      using (Stream stream = File.OpenRead(fileName))
      {
        this.Load(stream);
      }

      _fileName = fileName;
    }

    public virtual void Load(Stream stream)
    {
      TagReader reader;
      NbtFormat format;

      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      format = GetDocumentFormat(stream);
      reader = GetReader(format, stream);

      _documentRoot = reader.ReadDocument();
      _format = format;
    }

    public Tag Query(string query)
    {
      return this.Query<Tag>(query);
    }

    public T Query<T>(string query) where T : Tag
    {
      return _documentRoot.Query<T>(query);
    }

    public void Save()
    {
      this.Save(_fileName);
    }

    public void Save(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
      {
        throw new ArgumentNullException(nameof(fileName));
      }

      using (Stream stream = File.Create(fileName))
      {
        if (_format == NbtFormat.Binary)
        {
          // for binary files, compress with gzip
          using (GZipStream compressedStream = new GZipStream(stream, CompressionMode.Compress))
          {
            this.Save(compressedStream);
          }
        }
        else
        {
          // leave xml uncompressed
          this.Save(stream);
        }
      }

      _fileName = fileName;
    }

    public void Save(Stream stream)
    {
      TagWriter writer;

      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      writer = this.GetTagWriter(_format, stream);
      writer.WriteStartDocument();
      writer.WriteTag(_documentRoot);
      writer.WriteEndDocument();
      writer.Flush();
      writer.Close();
    }

    public override string ToString()
    {
      StringBuilder result;
      int indent;

      result = new StringBuilder();
      indent = -1;

      if (_documentRoot != null)
      {
        this.WriteTagString(_documentRoot, result, ref indent);
      }

      return result.ToString();
    }

    protected virtual TagWriter GetTagWriter(NbtFormat format, Stream stream)
    {
      return TagWriter.CreateWriter(format, stream);
    }

    private void WriteTagString(Tag tag, StringBuilder result, ref int indent)
    {
      ICollectionTag collection;
      ICollectionTag parentCollection;

      collection = tag as ICollectionTag;

      indent++;

      result.Append(new string(' ', indent * 2));

      result.Append(tag.Type.ToString().ToLowerInvariant());

      parentCollection = tag.Parent as ICollectionTag;
      if (parentCollection != null && parentCollection.IsList)
      {
        result.Append('#');
        result.Append(parentCollection.Values.IndexOf(tag));
      }
      else
      {
        result.Append(':');
        result.Append(tag.Name);
      }

      if (collection == null)
      {
        result.Append(" [").Append(tag.ToValueString()).Append(']');
      }

      result.AppendLine();

      if (collection != null)
      {
        foreach (Tag child in collection.Values)
        {
          this.WriteTagString(child, result, ref indent);
        }
      }

      indent--;
    }

    #endregion
  }
}
