namespace Cyotek.Data.Nbt
{
  public static partial class TagFactory
  {
    #region Static Methods

    public static Tag CreateTag(TagType tagType)
    {
      return CreateTag(string.Empty, tagType);
    }

    public static Tag CreateTag(string name, TagType tagType)
    {
      return CreateTag(name, tagType, TagType.None);
    }

    public static Tag CreateTag(TagType tagType, object value)
    {
      return CreateTag(string.Empty, tagType, value);
    }

    public static TagList CreateTag(TagType tagType, TagType listType)
    {
      return (TagList)CreateTag(string.Empty, tagType, listType);
    }

    #endregion
  }
}
