using System;
using System.ComponentModel;
using System.Globalization;

namespace Cyotek.Data.Nbt
{
  public sealed class TagByte : Tag, IEquatable<TagByte>
  {
    #region Fields

    private byte _value;

    #endregion

    #region Constructors

    public TagByte()
      : this(string.Empty, 0)
    { }

    public TagByte(string name)
      : this(name, 0)
    { }

    public TagByte(byte value)
      : this(string.Empty, value)
    { }

    public TagByte(string name, byte value)
      : base(name)
    {
      _value = value;
    }

    #endregion

    #region Properties

    public override TagType Type
    {
      get { return TagType.Byte; }
    }

    [Category("Data")]
    [DefaultValue((byte)0)]
    public byte Value
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
      _value = Convert.ToByte(value);
    }

    public override string ToValueString()
    {
      return _value.ToString(CultureInfo.InvariantCulture);
    }

    #endregion

    #region IEquatable<TagByte> Interface

    public bool Equals(TagByte other)
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
