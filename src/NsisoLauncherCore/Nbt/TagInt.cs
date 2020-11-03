using System;
using System.ComponentModel;
using System.Globalization;

namespace Cyotek.Data.Nbt
{
  public sealed class TagInt : Tag, IEquatable<TagInt>
  {
    #region Fields

    private int _value;

    #endregion

    #region Constructors

    public TagInt()
      : this(string.Empty, 0)
    { }

    public TagInt(string name)
      : this(name, 0)
    { }

    public TagInt(int value)
      : this(string.Empty, value)
    { }

    public TagInt(string name, int value)
      : base(name)
    {
      _value = value;
    }

    #endregion

    #region Properties

    public override TagType Type
    {
      get { return TagType.Int; }
    }

    [Category("Data")]
    [DefaultValue(0)]
    public int Value
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
      _value = Convert.ToInt32(value);
    }

    public override string ToValueString()
    {
      return _value.ToString(CultureInfo.InvariantCulture);
    }

    #endregion

    #region IEquatable<TagInt> Interface

    public bool Equals(TagInt other)
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
