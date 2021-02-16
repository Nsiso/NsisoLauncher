using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Cyotek.Data.Nbt
{
    public partial class TagDictionary : KeyedCollection<string, Tag>
    {
        #region Fields

        private Tag _owner;

        #endregion

        #region Properties

        public Tag Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;

                foreach (Tag child in this)
                {
                    child.Parent = value;
                }
            }
        }

        #endregion

        #region Methods

        public TagByte Add(string name, bool value)
        {
            return this.Add(name, (byte)(value ? 1 : 0));
        }

        public TagString Add(string name, DateTime value)
        {
            return this.Add(name, value.ToString("u"));
        }

        public TagByteArray Add(string name, Guid value)
        {
            return this.Add(name, value.ToByteArray());
        }

        /// <summary>
        /// Creates and adds a new <see cref="TagIntArray"/> with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the tag to add.</param>
        /// <param name="value">The value of the tag.</param>
        /// <returns>
        /// A <see cref="TagIntArray"/> containing the specified name and value.
        /// </returns>
        public TagList Add(string name, string[] value)
        {
            TagList tag;

            tag = (TagList)TagFactory.CreateTag(name, TagType.List, TagType.String);

            if (value?.Length > 0)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    tag.Value.Add(value[i]);
                }
            }

            this.Add(tag);

            return tag;
        }


        public Tag Add(string name, TagType tagType)
        {
            return this.Add(name, tagType, TagType.None);
        }

        public Tag Add(string name, TagType tagType, TagType limitToType)
        {
            Tag tag;

            tag = TagFactory.CreateTag(name, tagType, limitToType);

            this.Add(tag);

            return tag;
        }

        public Tag Add(string name, object value)
        {
            Tag result;

            if (value is byte byteValue)
            {
                result = this.Add(name, byteValue);
            }
            else if (value is int intValue)
            {
                result = this.Add(name, intValue);
            }
            else if (value is string stringValue)
            {
                result = this.Add(name, stringValue);
            }
            else if (value is bool boolValue)
            {
                result = this.Add(name, boolValue);
            }
            else if (value is float floatValue)
            {
                result = this.Add(name, floatValue);
            }
            else if (value is double doubleValue)
            {
                result = this.Add(name, doubleValue);
            }
            else if (value is long longValue)
            {
                result = this.Add(name, longValue);
            }
            else if (value is short shortValue)
            {
                result = this.Add(name, shortValue);
            }
            else if (value is DateTime dateTimeValue)
            {
                result = this.Add(name, dateTimeValue);
            }
            else if (value is string[] stringArrayValue)
            {
                result = this.Add(name, stringArrayValue);
            }
            else if (value is byte[] byteArrayValue)
            {
                result = this.Add(name, byteArrayValue);
            }
            else if (value is int[] intArrayValue)
            {
                result = this.Add(name, intArrayValue);
            }
            else if (value is long[] longArrayValue)
            {
                result = this.Add(name, longArrayValue);
            }
            else if (value is Guid guidValue)
            {
                result = this.Add(name, guidValue);
            }
            else if (value is TagDictionary tagDictionaryValue)
            {
                result = this.Add(name, tagDictionaryValue);
            }
            else if (value is TagCollection tagCollectionValue)
            {
                result = this.Add(name, tagCollectionValue);
            }
            else
            {
                throw new ArgumentException("Invalid value type.", nameof(value));
            }

            return result;
        }

        /// <summary>
        /// Adds a range of existing <see cref="T:KeyValuePair{string,object}"/> objects to the <see cref="TagDictionary"/>.
        /// </summary>
        /// <param name="values">An IEnumerable&lt;Tag&gt; of items to append to the <see cref="TagDictionary"/>.</param>
        public void AddRange(IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (KeyValuePair<string, object> value in values)
            {
                this.Add(value.Key, value.Value);
            }
        }

        /// <summary>
        /// Adds the contents of an existing <see cref="T:IDictionary{string,object}"/> objects to the <see cref="TagDictionary"/>.
        /// </summary>
        /// <param name="values">An IEnumerable&lt;Tag&gt; of items to append to the <see cref="TagDictionary"/>.</param>
        public void AddRange(IDictionary<string, object> values)
        {
            foreach (KeyValuePair<string, object> value in values)
            {
                this.Add(value.Key, value.Value);
            }
        }

        /// <summary>
        /// Adds a range of existing <see cref="Tag"/> objects to the <see cref="TagDictionary"/>.
        /// </summary>
        /// <param name="values">An IEnumerable&lt;Tag&gt; of items to append to the <see cref="TagDictionary"/>.</param>
        public void AddRange(IEnumerable<Tag> values)
        {
            foreach (Tag value in values)
            {
                this.Add(value);
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb;

            sb = new StringBuilder();

            sb.Append('[');

            foreach (Tag tag in this)
            {
                if (sb.Length > 1)
                {
                    sb.Append(',').Append(' ');
                }

                sb.Append(tag.ToValueString());
            }

            sb.Append(']');

            return sb.ToString();
        }

        public bool TryGetValue(string key, out Tag value)
        {
            bool result;

            if (this.Dictionary != null)
            {
                result = this.Dictionary.TryGetValue(key, out value);
            }
            else
            {
                result = false;
                value = null;
            }

            return result;
        }

        protected override void ClearItems()
        {
            foreach (Tag item in this)
            {
                item.Parent = null;
            }

            base.ClearItems();
        }

        protected override string GetKeyForItem(Tag item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, Tag item)
        {
            item.Parent = this.Owner;

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            Tag item;

            item = this[index];
            item.Parent = null;

            base.RemoveItem(index);
        }

        internal void ChangeKey(Tag item, string newKey)
        {
            this.ChangeItemKey(item, newKey);
        }

        #endregion
    }
}
