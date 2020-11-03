using System;
using System.ComponentModel;
using System.Globalization;

namespace Cyotek.Data.Nbt
{
  public sealed class TagShort : Tag, IEquatable<TagShort>
  {
    #region Fields

    private short _value;

    #endregion

    #region Constructors

    public TagShort()
      : this(string.Empty, 0)
    { }

    public TagShort(string name)
      : this(name, 0)
    { }

    public TagShort(short value)
      : this(string.Empty, value)
    { }

    public TagShort(string name, short value)
      : base(name)
    {
      _value = value;
    }

    #endregion

    #region Properties

    public override TagType Type
    {
      get { return TagType.Short; }
    }

    [Category("Data")]
    [DefaultValue((short)0)]
    public short Value
    {
      get { return _value; }
      set { _value = value; }
    }

    #endregion

    #region Methods

    public override int GetHashCode()
    {
      unchecked // Overflow is fine, just wrap
      {
        int hash;

        hash = 17;
        hash = hash * 23 + this.Name.GetHashCode();
        hash = hash * 23 + _value.GetHashCode();

        return hash;
      }
    }

    public override object GetValue()
    {
      return _value;
    }

    public override void SetValue(object value)
    {
      _value = Convert.ToInt16(value);
    }

    public override string ToValueString()
    {
      return _value.ToString(CultureInfo.InvariantCulture);
    }

    #endregion

    #region IEquatable<TagShort> Interface

    public bool Equals(TagShort other)
    {
      bool result;

      result = !ReferenceEquals(null, other);

      if (result && !ReferenceEquals(this, other))
      {
        result = string.Equals(this.Name, other.Name);

        if (result)
        {
          result = _value == other.Value;
        }
      }

      return result;
    }

    #endregion
  }
}
