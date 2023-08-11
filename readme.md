more detail see the Finbourne.InMemoryCache.xml file  that is alto genereted after a build, this is possible 
because I have added comments in all public and private methods and classes.

## Cache

The Cache class implements a Least Recently Used (LRU) cache with a specified capacity. The cache is thread-safe and can be used to store key-value pairs.

### Features

* The cache has a specified capacity. When the cache is full, the least recently used item is evicted.
* The cache supports an event called `ItemEvicted` which is raised when an item is evicted from the cache.
* The cache is thread-safe.

### Usage

To use the Cache class, you need to Get an instance of the class with the desired capacity. For example:

```
var cache = Cache<string, int>.GetInstance(3);
```
### Singleton Pattern

Cache class is applying Singleton Pattern so If you call GetInstance with different capacities, it will always use  the first created instance with the first specified capacity. For example:

```
var cache2 = Cache<string, int>.GetInstance(10);
// this wil thow a InvalidOperationException "Cannot change the capacity of the singleton instance once configured."
```

Once you have created an instance of the Cache class, you can add key-value pairs to the cache using the `Add` method. For example:

```
cache.Add("One", 1);
cache.Add("Two", 2);
cache.Add("Three", 3);
```

You can get a value from the cache by key using the `Get` method. For example:

```
var value = cache.Get("One");
```

You can try to remove a key-value pair from the cache using the `TryRemove` method. For example:

```
bool success = cache.TryRemove("Two");
```

You can get the capacity of the cache using the `Capacity` property. For example:

```
int capacity = cache.Capacity;
```

### Events

The Cache class raises an event called `ItemEvicted` when an item is evicted from the cache. The event handler receives the key and value of the evicted item. For example:

```
cache.ItemEvicted += (Key, Value) =>
{
    // Handle the event
    Console.WriteLine($"Item evicted: {Key}: {Value}");
};
```


### Clean Cache

Use the method Reset() to clean the cache.

```
cache.Reset();
```

### Method Count to get the total in the cache

```
cachse.Count;
```

### Example

The following code shows an example of how to use the Cache class:

```
using System;
using System.Collections.Generic;
using Finbourne.InMemoryCache;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var cache = Cache<string, int>.GetInstance(3);

            // Add some items to the cache
            cache.Add("One", 1);
            cache.Add("Two", 2);
            cache.Add("Three", 3);

            // Get a value from the cache by key
            var value = cache.Get("One");
            Console.WriteLine(value); // One

            // Get the capacity of the cache
            int capacity = cache.Capacity;
            Console.WriteLine(capacity); //3

            // Wait for the `ItemEvicted` event to be raised
            cache.ItemEvicted += (Key, Value) =>
            {
                // Handle the event
                Console.WriteLine($"Item evicted: {Key}: {Value}");
            };

            // Evict an item from the cache
            cache.Add("Four", 4);
  
            // Try to remove an item from the cache
            bool success = cache.TryRemove("One");
            Console.WriteLine(success); // True
        }
    }
}
```
