namespace Cyotek.Data.Nbt.Serialization
{
  public abstract partial class TagReader
  {
    #region Methods

    /// <summary>
    /// Closes the reader.
    /// </summary>
    public virtual void Close()
    { }

    public abstract bool IsNbtDocument();

    /// <summary>
    /// Reads a complete NBT document.
    /// </summary>
    /// <returns>
    /// A <see cref="TagCompound"/> containing the document contents.
    /// </returns>
    public TagCompound ReadDocument()
    {
      TagCompound tag;

      tag = (TagCompound)this.ReadTag();

      return tag;
    }

    public abstract Tag ReadTag();

    /// <summary>
    /// Reads the name of the next tag.
    /// </summary>
    public abstract string ReadTagName();

    /// <summary>
    /// Reads the type <see cref="TagType"/> of the next tag.
    /// </summary>
    public abstract TagType ReadTagType();

    #endregion
  }
}
