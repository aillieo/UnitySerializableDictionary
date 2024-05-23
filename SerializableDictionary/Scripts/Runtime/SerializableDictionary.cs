// -----------------------------------------------------------------------
// <copyright file="SerializableDictionary.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Represents a serializable dictionary that can be used in Unity projects.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    [Serializable]
    public abstract class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;

        [SerializeField]
        private bool invalidFlag;

        /// <inheritdoc/>
        public ICollection<TKey> Keys
        {
            get { return this.dictionary.Keys; }
        }

        /// <inheritdoc/>
        public ICollection<TValue> Values
        {
            get { return this.dictionary.Values; }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return (this.dictionary as ICollection<KeyValuePair<TKey, TValue>>).IsReadOnly; }
        }

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
            get { return this.dictionary[key]; }
            set { this.dictionary[key] = value; }
        }

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            this.dictionary.Add(key, value);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            return this.dictionary.Remove(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.dictionary.Clear();
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            (this.dictionary as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return (this.dictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        /// <inheritdoc/>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (this.dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return (this.dictionary as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public void OnBeforeSerialize()
        {
            if (this.invalidFlag)
            {
                return;
            }
            else
            {
                if (this.keys != null)
                {
                    this.keys.Clear();
                }

                if (this.values != null)
                {
                    this.values.Clear();
                }
            }

            if (this.dictionary.Count > 0)
            {
                if (this.keys == null)
                {
                    this.keys = new List<TKey>();
                }

                if (this.values == null)
                {
                    this.values = new List<TValue>();
                }
            }

            foreach (var pair in this.dictionary)
            {
                this.keys.Add(pair.Key);
                this.values.Add(pair.Value);
            }
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            this.dictionary.Clear();

            this.invalidFlag = false;

            if (this.keys.Count != this.values.Count)
            {
                var message = $"Invalid serialized data: {this.keys.Count} key(s) while {this.values.Count} value(s)";
#if UNITY_EDITOR
                Debug.LogWarning(message);
#else
                throw new Exception(message);
#endif
            }

            for (var i = 0; i < this.keys.Count; ++i)
            {
                if (!this.dictionary.ContainsKey(this.keys[i]))
                {
                    this.dictionary.Add(this.keys[i], this.values[i]);
                }
                else
                {
                    this.invalidFlag = true;
                    continue;
                }
            }

            if (!this.invalidFlag)
            {
                this.keys.Clear();
                this.values.Clear();
            }
        }
    }
}
