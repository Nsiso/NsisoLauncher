using System;
using System.ComponentModel;

namespace Cyotek.Data.Nbt
{
  public sealed class TagString : Tag, IEquatable<TagString>
  {
    #region Fields

    private string _value;

    #endregion

    #region Constructors

    public TagString()
      : this(string.Empty, string.Empty)
    { }

    public TagString(string name)
      : this(name, string.Empty)
    { }

    public TagString(string name, string value)
      : base(name)
    {
      _value = value;
    }

    #endregion

    #region Properties

    public override TagType Type
    {
      get { return TagType.String; }
    }

    [Category("Data")]
    [DefaultValue("")]
    public string Value
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
        hash = hash * 23 + this.ToString().GetHashCode();

        return hash;
      }
    }

    public override object GetValue()
    {
      return _value;
    }

    public override void SetValue(object value)
    {
      _value = Convert.ToString(value);
    }

    public override string ToString()
    {
      return string.Concat("[String: ", this.Name, "=\"", this.ToValueString(), "\"]");
    }

    public override string ToValueString()
    {
      return _value ?? string.Empty;
    }

    #endregion

    #region IEquatable<TagString> Interface

    public bool Equals(TagString other)
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
