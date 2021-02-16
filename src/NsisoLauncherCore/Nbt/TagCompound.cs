using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Cyotek.Data.Nbt
{
  public sealed class TagCompound : Tag, ICollectionTag, IEquatable<TagCompound>
  {
    #region Private Fields

    private static readonly byte[] _emptyByteArray = new byte[0];

    private static readonly int[] _emptyIntArray = new int[0];

    private static readonly long[] _emptyLongArray = new long[0];

    private static readonly string[] _emptyStringArray = new string[0];

    private static readonly char[] _queryDelimiters =
                {
      '\\',
      '/'
    };

    private TagDictionary _value;

    #endregion Private Fields

    #region Public Constructors

    public TagCompound()
      : this(string.Empty)
    { }

    public TagCompound(string name)
      : this(name, new TagDictionary())
    { }

    public TagCompound(TagDictionary value)
      : this(string.Empty, value)
    { }

    public TagCompound(string name, TagDictionary value)
      : base(name)
    {
      this.Value = value;
    }

    #endregion Public Constructors

    #region Public Properties

    /// <summary>
    /// Gets the number of child <see cref="Tag"/> objects actually contained in the <see cref="TagCompound"/>.
    /// </summary>
    public int Count
    {
      get { return _value.Count; }
    }

    /// <inheritdoc cref="Tag.Type"/>
    public override TagType Type
    {
      get { return TagType.Compound; }
    }

    [Category("Data")]
    [DefaultValue(typeof(TagDictionary), null)]
    public TagDictionary Value
    {
      get { return _value; }
      set
      {
        if (!ReferenceEquals(_value, value))
        {
          if (value == null)
          {
            throw new ArgumentNullException(nameof(value));
          }

          _value = value;
          value.Owner = this;
        }
      }
    }

    bool ICollectionTag.IsList
    {
      get { return false; }
    }

    TagType ICollectionTag.ListType
    {
      get { return TagType.None; }
      set { throw new NotSupportedException("Compounds cannot be restricted to a single type."); }
    }

    IList<Tag> ICollectionTag.Values
    {
      get { return this.Value; }
    }

    #endregion Public Properties

    #region Public Indexers

    /// <summary>
    /// Gets the <see cref="Tag"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the tag to get.</param>
    /// <returns>
    /// The <see cref="Tag"/> with the specified name. If a tag with the specified name is not found, an exception is thrown.
    /// </returns>
    public Tag this[string name]
    {
      get { return _value[name]; }
    }

    /// <summary>
    /// Gets the <see cref="Tag"/> at the specified index.
    /// </summary>
    /// <param name="index">Zero-based index of the entry to access.</param>
    /// <returns>
    /// The <see cref="Tag"/> at the specified index.
    /// </returns>
    public Tag this[int index]
    {
      get { return _value[index]; }
    }

    #endregion Public Indexers

    #region Public Methods

    public bool Contains(string name)
    {
      return this.Value.Contains(name);
    }

    public bool Equals(TagCompound other)
    {
      bool result;

      result = !ReferenceEquals(null, other);

      if (result && !ReferenceEquals(this, other))
      {
        result = string.Equals(this.Name, other.Name);

        if (result)
        {
          IList<Tag> src;
          IList<Tag> dst;

          src = this.Value;
          dst = other.Value;

          result = src.Count == dst.Count;

          for (int i = 0; i < src.Count; i++)
          {
            Tag srcTag;
            Tag dstTag;

            srcTag = src[i];
            dstTag = dst[i];

            if (!srcTag.Equals(dstTag))
            {
              result = false;
              break;
            }
          }
        }
      }

      return result;
    }

    public bool GetBooleanValue(string name)
    {
      return this.GetBooleanValue(name, false);
    }

    public bool GetBooleanValue(string name, bool defaultValue)
    {
      TagByte value;

      value = this.GetTag<TagByte>(name);

      return value != null ? value.Value != 0 : defaultValue;
    }

    public TagByte GetByte(string name)
    {
      return this.GetTag<TagByte>(name);
    }

    public TagByteArray GetByteArray(string name)
    {
      return this.GetTag<TagByteArray>(name);
    }

    public byte[] GetByteArrayValue(string name)
    {
      return this.GetByteArrayValue(name, _emptyByteArray);
    }

    public byte[] GetByteArrayValue(string name, byte[] defaultValue)
    {
      TagByteArray value;

      value = this.GetTag<TagByteArray>(name);

      return value != null ? value.Value : defaultValue;
    }

    public byte GetByteValue(string name)
    {
      return this.GetByteValue(name, default(byte));
    }

    public byte GetByteValue(string name, byte defaultValue)
    {
      TagByte value;

      value = this.GetTag<TagByte>(name);

      return value?.Value ?? defaultValue;
    }

    public TagCompound GetCompound(string name)
    {
      return this.GetTag<TagCompound>(name);
    }

    public DateTime GetDateTimeValue(string name)
    {
      return this.GetDateTimeValue(name, DateTime.MinValue);
    }

    public DateTime GetDateTimeValue(string name, DateTime defaultValue)
    {
      TagString value;

      value = this.GetTag<TagString>(name);

      return value != null ? DateTime.Parse(value.Value, CultureInfo.InvariantCulture).ToUniversalTime() : defaultValue;
    }

    public TagDouble GetDouble(string name)
    {
      return this.GetTag<TagDouble>(name);
    }

    public double GetDoubleValue(string name)
    {
      return this.GetDoubleValue(name, 0);
    }

    public double GetDoubleValue(string name, double defaultValue)
    {
      TagDouble value;

      value = this.GetTag<TagDouble>(name);

      return value?.Value ?? defaultValue;
    }

    public TagFloat GetFloat(string name)
    {
      return this.GetTag<TagFloat>(name);
    }

    public float GetFloatValue(string name)
    {
      return this.GetFloatValue(name, 0);
    }

    public float GetFloatValue(string name, float defaultValue)
    {
      TagFloat value;

      value = this.GetTag<TagFloat>(name);

      return value?.Value ?? defaultValue;
    }

    public Guid GetGuidValue(string name)
    {
      return this.GetGuidValue(name, Guid.Empty);
    }

    public Guid GetGuidValue(string name, Guid defaultValue)
    {
      TagByteArray tag;

      tag = this.GetByteArray(name);

      return tag != null ? new Guid(tag.Value) : defaultValue;
    }

    public override int GetHashCode()
    {
      // http://stackoverflow.com/a/263416/148962

      unchecked // Overflow is fine, just wrap
      {
        int hash;
        TagDictionary values;

        hash = 17;
        hash = hash * 23 + this.Name.GetHashCode();

        values = this.Value;

        if (values != null)
        {
          for (int i = 0; i < values.Count; i++)
          {
            hash = hash * 23 + _value[i].GetHashCode();
          }
        }

        return hash;
      }
    }

    public TagInt GetInt(string name)
    {
      return this.GetTag<TagInt>(name);
    }

    public TagIntArray GetIntArray(string name)
    {
      return this.GetTag<TagIntArray>(name);
    }

    public int[] GetIntArrayValue(string name)
    {
      return this.GetIntArrayValue(name, _emptyIntArray);
    }

    public int[] GetIntArrayValue(string name, int[] defaultValue)
    {
      TagIntArray value;

      value = this.GetTag<TagIntArray>(name);

      return value != null ? value.Value : defaultValue;
    }

    public TagLongArray GetLongArray(string name)
    {
      return this.GetTag<TagLongArray>(name);
    }

    public long[] GetLongArrayValue(string name)
    {
      return this.GetLongArrayValue(name, _emptyLongArray);
    }

    public long[] GetLongArrayValue(string name, long[] defaultValue)
    {
      TagLongArray value;

      value = this.GetTag<TagLongArray>(name);

      return value != null ? value.Value : defaultValue;
    }

    public int GetIntValue(string name)
    {
      return this.GetIntValue(name, 0);
    }

    public int GetIntValue(string name, int defaultValue)
    {
      TagInt value;

      value = this.GetTag<TagInt>(name);

      return value?.Value ?? defaultValue;
    }

    public TagList GetList(string name)
    {
      return this.GetTag<TagList>(name);
    }

    public TagLong GetLong(string name)
    {
      return this.GetTag<TagLong>(name);
    }

    public long GetLongValue(string name)
    {
      return this.GetLongValue(name, 0);
    }

    public long GetLongValue(string name, long defaultValue)
    {
      TagLong value;

      value = this.GetTag<TagLong>(name);

      return value?.Value ?? defaultValue;
    }

    public TagShort GetShort(string name)
    {
      return this.GetTag<TagShort>(name);
    }

    public short GetShortValue(string name)
    {
      return this.GetShortValue(name, 0);
    }

    public short GetShortValue(string name, short defaultValue)
    {
      TagShort value;

      value = this.GetTag<TagShort>(name);

      return value?.Value ?? defaultValue;
    }

    public TagString GetString(string name)
    {
      return this.GetTag<TagString>(name);
    }

    public string[] GetStringArrayValue(string name)
    {
      return this.GetStringArrayValue(name, _emptyStringArray);
    }

    public string[] GetStringArrayValue(string name, string[] defaultValue)
    {
      TagList value;
      string[] result;

      value = this.GetTag<TagList>(name);

      if (value != null)
      {
        if (value.Count == 0)
        {
          result = _emptyStringArray;
        }
        else
        {
          result = new string[value.Count];

          for (int i = 0; i < result.Length; i++)
          {
            result[i] = ((TagString)value.Value[i]).Value;
          }
        }
      }
      else
      {
        result = defaultValue;
      }

      return result;
    }

    public string GetStringValue(string name)
    {
      return this.GetStringValue(name, null);
    }

    public string GetStringValue(string name, string defaultValue)
    {
      TagString value;

      value = this.GetTag<TagString>(name);

      return value != null ? value.Value : defaultValue;
    }

    public T GetTag<T>(string name) where T : Tag
    {
      Tag value;

      this.Value.TryGetValue(name, out value);

      return (T)value;
    }

    public Tag GetTag(string name)
    {
      return this.GetTag<Tag>(name);
    }

    public override object GetValue()
    {
      return _value;
    }

    public Tag Query(string query)
    {
      return this.Query<Tag>(query);
    }

    public T Query<T>(string query) where T : Tag
    {
      string[] parts;
      Tag element;
      bool failed;

      parts = query.Split(_queryDelimiters);
      element = this;
      failed = false;

      // HACK: This is all quickly thrown together

      foreach (string part in parts)
      {
        if (part.IndexOf('[') != -1)
        {
          int attributePosition;
          bool matchFound;

          attributePosition = part.IndexOf('=');
          matchFound = false;

          if (attributePosition != -1)
          {
            string name;
            string value;
            TagList list;

            name = part.Substring(1, attributePosition - 1);
            value = part.Substring(attributePosition + 1, part.Length - (attributePosition + 2));
            list = element as TagList;

            if (list != null)
            {
              // ReSharper disable once LoopCanBePartlyConvertedToQuery
              foreach (Tag tag in list.Value)
              {
                TagCompound compound;

                compound = tag as TagCompound;

                if (compound != null && compound.GetStringValue(name) == value)
                {
                  element = tag;
                  matchFound = true;
                  break;
                }
              }
            }
          }

          if (!matchFound)
          {
            // attribute not found or not set
            failed = true;
            break;
          }
        }
        else
        {
          ICollectionTag container;

          container = element as ICollectionTag;

          if (container != null && container.IsList)
          {
            // list entry
            int index;

            if (int.TryParse(part, out index) && index < container.Values.Count)
            {
              element = container.Values[index];
            }
            else
            {
              // invalid index, or out of bounds
              failed = true;
              break;
            }
          }
          else
          {
            // compound
            TagCompound compound;

            compound = (TagCompound)element;

            if (!compound.Value.TryGetValue(part, out element))
            {
              // didn't find a matching key
              failed = true;
              break;
            }
          }
        }
      }

      return !failed ? (T)element : null;
    }

    public T QueryValue<T>(string query)
    {
      return this.QueryValue(query, default(T));
    }

    public T QueryValue<T>(string query, T defaultValue)
    {
      Tag tag;

      tag = this.Query<Tag>(query);

      return tag != null ? (T)tag.GetValue() : defaultValue;
    }

    public override void SetValue(object value)
    {
      this.Value = (TagDictionary)value;
    }

    public override string ToString()
    {
      int count;

      count = _value?.Count ?? 0;

      return string.Concat("[", this.Type, ": ", this.Name, "] (", count.ToString(CultureInfo.InvariantCulture), " items)");
    }

    public override string ToValueString()
    {
      return _value?.ToString() ?? string.Empty;
    }

    #endregion Public Methods
  }
}
