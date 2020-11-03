using System;
using System.Collections.Generic;
using System.IO;

namespace Cyotek.Data.Nbt.Serialization
{
  /// <summary>
  /// Helper class for keep container state.
  /// </summary>
  internal sealed class TagState
  {
    #region Constants

    private readonly FileAccess _mode;

    #endregion

    #region Fields

    private Stack<TagContainerState> _openContainers;

    private Stack<TagType> _openTags;

    #endregion

    #region Constructors

    public TagState(FileAccess mode)
    {
      _mode = mode;
    }

    #endregion

    #region Properties

    public TagType CurrentTag
    {
      get
      {
        TagType type;

        type = _openTags != null && _openTags.Count != 0 ? _openTags.Peek() : TagType.None;

        return type;
      }
    }

    #endregion

    #region Methods

    public void EndTag()
    {
      this.EndTag(null);
    }

    public void EndTag(Action writeEnd)
    {
      TagType type;

      if (_openTags == null)
      {
        throw new InvalidOperationException("No document is currently open.");
      }

      if (_openTags.Count == 0)
      {
        throw new InvalidOperationException("No tag is currently open.");
      }

      type = _openTags.Pop();

      if (type == TagType.List || type == TagType.Compound)
      {
        TagContainerState state;

        state = _openContainers.Pop();

        if (type == TagType.Compound)
        {
          writeEnd?.Invoke();
        }
        else if (_mode == FileAccess.Write && state.ChildCount != state.ExpectedCount)
        {
          throw new InvalidOperationException($"Expected {state.ExpectedCount} children, but {state.ChildCount} were written.");
        }
      }
    }

    public void SetComplete()
    {
      if (_openTags == null)
      {
        throw new InvalidOperationException("No document is currently open.");
      }

      _openTags = null;
      _openContainers = null;
    }

    public void Start()
    {
      if (_openTags != null)
      {
        throw new InvalidOperationException("Document is already open.");
      }

      _openTags = new Stack<TagType>();
      _openContainers = new Stack<TagContainerState>();
    }

    public void StartList(TagType listType, int expectedCount)
    {
      if (_openContainers.Count != 0)
      {
        TagContainerState state;

        state = _openContainers.Peek();
        state.ChildType = listType;
        state.ExpectedCount = expectedCount;
      }
    }

    public TagContainerState StartTag(TagType type)
    {
      TagContainerState currentState;

      if (_openTags == null)
      {
        throw new InvalidOperationException("No document is currently open.");
      }

      if (_openTags.Count != 0)
      {
        currentState = _openContainers.Peek();

        if (currentState.ContainerType == TagType.List && currentState.ChildType != TagType.End && type != currentState.ChildType)
        {
          throw new InvalidOperationException($"Attempted to add tag of type '{type}' to container that only accepts '{currentState.ChildType}'.");
        }

        currentState.ChildCount++;
      }
      else
      {
        currentState = null;
      }

      _openTags.Push(type);

      if (type == TagType.Compound || type == TagType.List)
      {
        _openContainers.Push(new TagContainerState
                             {
                               ContainerType = type
                             });
      }

      return currentState;
    }

    #endregion
  }
}
