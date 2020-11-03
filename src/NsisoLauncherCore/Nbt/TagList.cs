using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Cyotek.Data.Nbt
{
  public sealed class TagList : Tag, ICollectionTag, IEquatable<TagList>
  {
    #region Fields

    private TagCollection _value;

    #endregion

    #region Constructors

    public TagList()
      : this(string.Empty)
    { }

    public TagList(string name)
      : this(name, TagType.None)
    { }

    public TagList(TagType listType)
      : this(string.Empty, listType)
    { }

    public TagList(TagCollection value)
      : this(string.Empty, value)
    { }

    public TagList(string name, TagType listType)
      : this(name, listType, new TagCollection(listType))
    { }

    public TagList(string name, TagCollection value)
      : this(name, value.LimitType, value)
    { }

    public TagList(string name, TagType listType, TagCollection value)
      : base(name)
    {
      this.ListType = listType;
      this.Value = value;
    }

    #endregion

    #region Properties

    public int Count
    {
      get { return _value.Count; }
    }

    public override TagType Type
    {
      get { return TagType.List; }
    }

    [Category("Data")]
    [DefaultValue(typeof(TagCollection), null)]
    public TagCollection Value
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

    #endregion

    #region Methods

    public override int GetHashCode()
    {
      // http://stackoverflow.com/a/263416/148962

      unchecked // Overflow is fine, just wrap
      {
        int hash;
        TagCollection values;

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

    public override object GetValue()
    {
      return _value;
    }

    public override void SetValue(object value)
    {
      this.Value = (TagCollection)value;
    }

    public override string ToString()
    {
      int count;

      count = _value.Count;

      return string.Concat("[", this.Type, ": ", this.Name, "] (", count.ToString(CultureInfo.InvariantCulture), " items)");
    }

    public override string ToValueString()
    {
      return _value.ToString() ?? string.Empty;
    }

    #endregion

    #region ICollectionTag Interface

    public TagType ListType
    {
      get { return this.Value?.LimitType ?? TagType.None; }
      set
      {
        if (this.Value == null || _value.LimitType != value)
        {
          this.Value = new TagCollection(value);
        }
      }
    }

    bool ICollectionTag.IsList
    {
      get { return true; }
    }

    IList<Tag> ICollectionTag.Values
    {
      get { return this.Value; }
    }

    #endregion

    #region IEquatable<TagList> Interface

    public bool Equals(TagList other)
    {
      bool result;

      result = !ReferenceEquals(null, other);

      if (result && !ReferenceEquals(this, other))
      {
        result = string.Equals(this.Name, other.Name) && this.ListType == other.ListType;

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

    #endregion
  }
}
