using System.Collections.Generic;

namespace Cyotek.Data.Nbt
{
  public interface ICollectionTag
  {
    #region Properties

    bool IsList { get; }

    TagType ListType { get; set; }

    IList<Tag> Values { get; }

    #endregion
  }
}
