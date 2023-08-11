using System;
using System.Collections.Generic;
using Finbourne.InMemoryCache.Interfaces;

namespace Finbourne.InMemoryCache.Implementations
{
    /// <summary>
    /// Implemen a Least Recently Used (LRU) cache with a specified capacity.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the cache.</typeparam>
    /// <typeparam name="TValue">The type of the values in the cache.</typeparam>
    public sealed class Cache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
    {
        /// <summary>
        /// this ItemEvicted event is raised when an item is evicted from the cache.
        /// </summary>
        public event Action<TKey, TValue>? ItemEvicted;

        /// <summary>
        /// Maximum number of items that can be stored in the cache.
        /// </summary>
        private readonly int capacity;

        /// <summary>
        /// to keep track of the configured capacity of the cache.
        /// </summary>
        private static int? configuredCapacity = null;

        /// <summary>
        /// Dictionary to hold the cache items and enable fast lookup.
        /// </summary>
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cacheMap;

        /// <summary>
        /// Linked list t maintain the order of access for cache items.
        /// </summary>
        private readonly LinkedList<CacheItem> lruList;

        /// <summary>
        /// A lock object to make sure that only one thread can access the cache at a time
        /// </summary>
        private readonly object syncLock = new object();

        /// <summary>
        /// a lock object to make sure that only one thread can access the instance at a time
        /// </summary>
        private static volatile Cache<TKey, TValue>? instance;

        /// <summary>
        /// a lock object to make sure that only one thread can access the instance at a time
        /// </summary>
        private static readonly object instanceLock = new object();

    

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{TKey, TValue}"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of items that can be stored in the cache.</param>
        private Cache(int capacity)
        {
            this.capacity = capacity;
            this.cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
            this.lruList = new LinkedList<CacheItem>();
        }

        /// <summary>
        /// this method creates a new instance of the cache if it does not exist, or returns the existing instance.
        /// </summary>
        /// <param name="capacity">capacity of the cache</param>
        /// <returns>returns the instance of the cache</returns> 
        public static Cache<TKey, TValue> GetInstance(int capacity)
        {
            if (instance != null && configuredCapacity != capacity)
            {
                throw new InvalidOperationException("Cannot change the capacity of the singleton instance once configured.");
            }

            if (instance == null)
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        configuredCapacity = capacity; // Set the capacity for the first time
                        instance = new Cache<TKey, TValue>(capacity);
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Adds a key-value pair to the cache. If the key already exists, the value is updated and moved to the front.
        /// If adding a new item causes the cache to exceed its capacity, the least recently used item is evicted.
        /// If an item is evicted, the ItemEvicted event is raised.
        /// </summary>
        /// <param name="key">The key to add or update in the cache.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        /// <remarks>
        /// 1. If the key already exists in the cache, the value is updated, and the item is moved to the front of the cache.
        /// 2. If the key does not exist and the cache is at full capacity, the least recently used item is evicted.
        /// 3. If an item is evicted, the ItemEvicted event is raised.
        /// 4. If the key does not exist and there is room in the cache, the new key-value pair is added to the cache.
        /// </remarks>
        public void Add(TKey key, TValue value)
        {
            Action<TKey, TValue>? evictedEvent = null;
            TKey? evictedKey = default;
            TValue? evictedValue = default;

            lock (syncLock)
            {
                if (cacheMap.ContainsKey(key))
                {
                    UpdateExistingItem(key, value);
                }
                else
                {
                    if (cacheMap.Count >= capacity)
                    {
                        var lastNode = lruList.Last;
                        evictedKey = lastNode.Value.Key;
                        evictedValue = lastNode.Value.Value;
                        EvictLastItem();

                        // Create a local copy of the event
                        evictedEvent = ItemEvicted;
                    }

                    AddNewItem(key, value);
                }
            }

            // Invoke the event outside the lock
            if (evictedKey != null && evictedValue != null)
            {
                evictedEvent?.Invoke(evictedKey, evictedValue);
            }
        }

        /// <summary>
        /// this method gets a value from the cache and Move accessed item to the front
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TValue Get(TKey key)
        {
            lock (syncLock)
            {
                if (cacheMap.TryGetValue(key, out var node))
                {
                    lruList.Remove(node);
                    lruList.AddFirst(node);
                    return node.Value.Value;
                }

                throw new KeyNotFoundException($"The key {key} was not found in the cache.");
            }
        }

        /// <summary>
        /// this method removes a key value pair from the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryRemove(TKey key)
        {
            lock (syncLock)
            {
                if (cacheMap.TryGetValue(key, out var node))
                {
                    lruList.Remove(node);
                    cacheMap.Remove(key);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// returns the capacity of the cache
        /// </summary>
        public int Capacity
        {
            get { return capacity; }
        }

 
        /// <summary>
        /// this methos clean the cache
        /// </summary>
        public void Reset()
        {
            lock (syncLock)
            {
                cacheMap.Clear();
                lruList.Clear();
            }
        }

        /// <summary>
        /// this method returns the number of items in the cache
        /// </summary>
        public int Count
        {
            get
            {
                lock (syncLock)
                {
                    return cacheMap.Count;
                }
            }
        }

        /// <summary>
        /// this method updates an existing item in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void UpdateExistingItem(TKey key, TValue value)
        {
            var existingNode = cacheMap[key];
            lruList.Remove(existingNode);
            var updatedItem = new CacheItem(key, value);
            var updatedNode = new LinkedListNode<CacheItem>(updatedItem);
            lruList.AddFirst(updatedNode);
            cacheMap[key] = updatedNode;
        }

        /// <summary>
        /// this method adds a new item to the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AddNewItem(TKey key, TValue value)
        {
            var cacheItem = new CacheItem(key, value);
            var node = new LinkedListNode<CacheItem>(cacheItem);
            lruList.AddFirst(node);
            cacheMap.Add(key, node);
        }

        /// <summary>
        /// this method evicts the last item in the cache
        /// </summary>
        private void EvictLastItem()
        {
            var lastNode = lruList.Last;
            cacheMap.Remove(lastNode.Value.Key);
            lruList.RemoveLast();
        }

        /// <summary>
        /// this class represents an item in the cache
        /// </summary>
        private class CacheItem
        {
            /// <summary>
            /// Gets the key of the cache item.
            /// </summary>
            public TKey Key { get; }

            /// <summary>
            /// Gets the value of the cache item.
            /// </summary>
            public TValue Value { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CacheItem"/> class with the specified key and value.
            /// </summary>
            /// <param name="key">The key of the cache item.</param>
            /// <param name="value">The value of the cache item.</param>
            public CacheItem(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
