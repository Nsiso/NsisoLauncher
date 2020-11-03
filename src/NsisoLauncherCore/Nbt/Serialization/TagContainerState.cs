namespace Cyotek.Data.Nbt.Serialization
{
  /// <summary>
  /// Describes the state of a operating involving a tag container.
  /// </summary>
  internal sealed class TagContainerState
  {
    #region Fields

    /// <summary>
    /// Number of children.
    /// </summary>
    public int ChildCount;

    /// <summary>
    /// Type of children.
    /// </summary>
    public TagType ChildType;

    /// <summary>
    /// Container type.
    /// </summary>
    public TagType ContainerType;

    /// <summary>
    /// Expected number of children.
    /// </summary>
    public int ExpectedCount;

    #endregion
  }
}
