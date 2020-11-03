using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Cyotek.Data.Nbt
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  // Disabling 659 this as it's pointless overriding
  // GetHashCode when each concrete instance already
  // is, making a base version uncallable
  public abstract class Tag : IEquatable<Tag>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    #region Fields

    private string _name;

    private Tag _parent;

    #endregion

    #region Constructors

    /// <summary>
    /// Specialised constructor for use only by derived class.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    protected Tag(string name)
    {
      _name = name ?? string.Empty;
    }

    #endregion

    #region Properties

    [Browsable(false)]
    public string FullPath
    {
      get
      {
        Tag[] ancestors;
        StringBuilder sb;

        ancestors = this.GetAncestors();
        sb = new StringBuilder();

        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < ancestors.Length; i++)
        {
          Tag ancestor;
          ICollectionTag container;

          ancestor = ancestors[i];
          container = ancestor.Parent as ICollectionTag;

          if (sb.Length != 0)
          {
            sb.Append('\\');
          }

          if (container == null || !container.IsList)
          {
            sb.Append(ancestor.Name);
          }
          else
          {
            sb.Append(container.Values.IndexOf(ancestor));
          }
        }

        if (sb.Length != 0)
        {
          sb.Append('\\');
        }

        sb.Append(_name);

        return sb.ToString();
      }
    }

    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    /// <value>
    /// The name of the tag.
    /// </value>
    [Category("Data")]
    [DefaultValue("")]
    public string Name
    {
      get { return _name; }
      set
      {
        if (value == null)
        {
          value = string.Empty;
        }

        if (this.Name != value)
        {
          ICollectionTag collection;
          TagDictionary values;

          collection = _parent as ICollectionTag;
          values = collection?.Values as TagDictionary;

          values?.ChangeKey(this, value);

          _name = value;
        }
      }
    }

    /// <summary>
    /// Gets the parent <see cref="Tag"/>.
    /// </summary>
    /// <value>
    /// The parent <see cref="Tag"/>.
    /// </value>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Tag Parent
    {
      get { return _parent; }
      internal set { _parent = value; }
    }

    /// <summary>
    /// Gets the tag type.
    /// </summary>
    /// <value>
    /// The tag type.
    /// </value>
    public abstract TagType Type { get; }

    #endregion

    #region Methods

#pragma warning disable 659

    /// <summary>
    /// Tests if this <see cref="Tag"/> is considered equal to another.
    /// </summary>
    /// <param name="obj">The object to compare to this object.</param>
    /// <returns>
    /// <c>true</c> if the objects are considered equal, <c>false</c> if they are not.
    /// </returns>
    /// <seealso cref="M:Cyotek.Data.Nbt.Tag.Equals(object)"/>
    public override bool Equals(object obj)
#pragma warning restore 659
    {
      return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && this.Equals((Tag)obj));
    }

    public Tag[] Flatten()
    {
      Tag[] results;

      if (!(this is ICollectionTag))
      {
        // single value
        results = new[]
                  {
                    this
                  };
      }
      else
      {
        // multiple values;
        List<Tag> tags;

        tags = new List<Tag>();

        this.FlattenTag(this, tags);

        results = tags.ToArray();
      }

      return results;
    }

    public Tag[] GetAncestors()
    {
      Tag[] results;
      List<Tag> tags;
      Tag tag;
      int arrayIndex;

      tags = new List<Tag>();
      tag = _parent;

      while (tag != null)
      {
        tags.Add(tag);
        tag = tag.Parent;
      }

      results = new Tag[tags.Count];
      arrayIndex = 0;

      for (int i = tags.Count; i > 0; i--)
      {
        results[arrayIndex++] = tags[i - 1];
      }

      return results;
    }

    /// <summary>
    /// Gets the value of a tag.
    /// </summary>
    /// <returns>
    /// The tag's value.
    /// </returns>
    /// <remarks>Where possible, it is recommended the <c>Value</c> property of a tag is used to avoid boxing.</remarks>
    public abstract object GetValue();

    /// <summary>
    /// Sets the value of the tag.
    /// </summary>
    /// <param name="value">The new value of the tag.</param>
    /// <remarks>Where possible, it is recommended the <c>Value</c> property of a tag is used to avoid boxing.</remarks>
    public abstract void SetValue(object value);

    /// <summary>
    /// Convert this object into a string representation.
    /// </summary>
    /// <returns>
    /// A string that represents this object.
    /// </returns>
    public override string ToString()
    {
      return string.Concat("[", this.Type, ": ", this.Name, "=", this.ToValueString(), "]");
    }

    /// <summary>
    /// Converts the value of this object to a string.
    /// </summary>
    /// <returns>
    /// The value of this object as a string.
    /// </returns>
    public abstract string ToValueString();

    private void FlattenTag(Tag tag, ICollection<Tag> tags)
    {
      ICollectionTag collectionTag;

      tags.Add(tag);

      collectionTag = tag as ICollectionTag;
      if (collectionTag != null)
      {
        foreach (Tag childTag in collectionTag.Values)
        {
          this.FlattenTag(childTag, tags);
        }
      }
    }

    #endregion

    #region IEquatable<Tag> Interface

    /// <summary>
    /// Tests if this <see cref="Tag"/> is considered equal to another.
    /// </summary>
    /// <param name="other">The tag to compare to this object.</param>
    /// <returns>
    /// <c>true</c> if the objects are considered equal, <c>false</c> if they are not.
    /// </returns>
    public bool Equals(Tag other)
    {
      bool result;

      result = !ReferenceEquals(null, other);

      if (result && !ReferenceEquals(this, other))
      {
        result = string.Equals(_name, other.Name);

        if (result)
        {
          result = Equals(this.GetValue(), other.GetValue());
        }
      }

      return result;
    }

    #endregion
  }
}
