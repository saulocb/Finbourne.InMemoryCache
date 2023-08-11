using System;
using System.Collections.Generic;

namespace Finbourne.InMemoryCache.Interfaces
{
    /// <summary>
    /// Defines the methods for a cache with a specified capacity.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache.</typeparam>
    public interface ICache<TKey, TValue> where TKey : notnull
    {
        /// <summary>
        /// An event that is raised when an item is evicted from the cache.
        /// </summary>
        event Action<TKey, TValue> ItemEvicted;

        /// <summary>
        /// Adds a key-value pair to the cache.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Gets a value from the cache by key.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        TValue Get(TKey key);

        /// <summary>
        /// Attempts to remove a key-value pair from the cache.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        bool TryRemove(TKey key);

        /// <summary>
        /// this method is used to reset the cache
        /// </summary>
        public void Reset();

        /// <summary>
        /// this method is used to get the count of the cache
        /// </summary>
        /// <returns></returns>
        public int Count { get; }

        /// <summary>
        /// Gets the capacity of the cache.
        /// </summary>
        int Capacity { get; }
    }
}
