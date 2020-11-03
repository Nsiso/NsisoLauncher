using System.Text;

namespace Cyotek.Data.Nbt
{
  public sealed partial class TagByteArray
  {
    #region Methods

    public override string ToValueString()
    {
      StringBuilder sb;

      sb = new StringBuilder();

      // ReSharper disable once ForCanBeConvertedToForeach
      for (int i = 0; i < _value.Length; i++)
      {
        if (sb.Length != 0)
        {
          sb.Append(", ");
        }

        sb.Append(_value[i].ToString("X2"));
      }

      return sb.ToString();
    }

    #endregion
  }
}
