using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Cyotek.Data.Nbt
{
  public partial class TagCollection : Collection<Tag>
  {
    #region Fields

    private TagType _limitType;

    private Tag _owner;

    #endregion

    #region Constructors

    public TagCollection()
    {
      _limitType = TagType.None;
    }

    public TagCollection(TagType limitType)
    {
      _limitType = limitType;
    }

    #endregion

    #region Properties

    public TagType LimitType
    {
      get { return _limitType; }
    }

    public Tag Owner
    {
      get { return _owner; }
      set
      {
        _owner = value;

        foreach (Tag child in this)
        {
          child.Parent = value;
        }
      }
    }

    #endregion

    #region Methods

    public Tag Add(TagType tagType)
    {
      return this.Add(tagType, TagType.None);
    }

    public Tag Add(TagType tagType, TagType limitToType)
    {
      Tag tag;

      tag = TagFactory.CreateTag(string.Empty, tagType, limitToType);

      this.Add(tag);

      return tag;
    }

    public new void Add(Tag value)
    {
      base.Add(value);
    }

    public Tag Add(object value)
    {
      Tag result;

      if (value is byte byteValue)
      {
        result = this.Add(byteValue);
      }
      else if (value is int intValue)
      {
        result = this.Add(intValue);
      }
      else if (value is float floatValue)
      {
        result = this.Add(floatValue);
      }
      else if (value is double doubleValue)
      {
        result = this.Add(doubleValue);
      }
      else if (value is long longValue)
      {
        result = this.Add(longValue);
      }
      else if (value is short shortValue)
      {
        result = this.Add(shortValue);
      }
      else if (value is string stringValue)
      {
        result = this.Add(stringValue);
      }
      else if (value is TagDictionary tagDictionary)
      {
        result = this.Add(tagDictionary);
      }
      else if (value is TagCollection tagCollection)
      {
        result = this.Add(tagCollection);
      }
      else if (value is byte[] byteArrayValue)
      {
        result = this.Add(byteArrayValue);
      }
      else if (value is int[] intArrayValue)
      {
        result = this.Add(intArrayValue);
      }
      else if (value is long[] longArrayValue)
      {
        result = this.Add(longArrayValue);
      }
      else
      {
        throw new ArgumentException("Invalid value type.", nameof(value));
      }

      return result;
    }

    public void AddRange(IEnumerable<object> values)
    {
      foreach (object value in values)
      {
        this.Add(value);
      }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    public override string ToString()
    {
      StringBuilder sb;

      sb = new StringBuilder();

      sb.Append('[');

      foreach (Tag tag in this)
      {
        if (sb.Length > 1)
        {
          sb.Append(',').Append(' ');
        }

        sb.Append(tag.ToValueString());
      }

      sb.Append(']');

      return sb.ToString();
    }

    protected override void ClearItems()
    {
      foreach (Tag item in this)
      {
        item.Parent = null;
      }

      base.ClearItems();
    }

    protected override void InsertItem(int index, Tag item)
    {
      if (_limitType == TagType.None)
      {
        _limitType = item.Type;
      }
      else if (item.Type != _limitType)
      {
        throw new ArgumentException($"Only items of type {_limitType} can be added to this collection.", nameof(item));
      }

      if (!string.IsNullOrEmpty(item.Name))
      {
        throw new ArgumentException("Only unnamed tags are supported.", nameof(item));
      }

      item.Parent = this.Owner;

      base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
      Tag item;

      item = this[index];
      item.Parent = null;

      base.RemoveItem(index);
    }

    protected override void SetItem(int index, Tag item)
    {
      if (_limitType != TagType.None && item.Type != _limitType)
      {
        throw new ArgumentException($"Only items of type {_limitType} can be added to this collection.", nameof(item));
      }

      item.Parent = this.Owner;

      base.SetItem(index, item);
    }

    #endregion
  }
}
