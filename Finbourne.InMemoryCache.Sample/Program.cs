using Finbourne.InMemoryCache.Implementations;
using System;

namespace LRUCacheSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of Cache with a capacity of 3
            var cache = Cache<string, int>.GetInstance(3);


            // Subscribe to the ItemEvicted event
            cache.ItemEvicted += (key, value) =>
            {
                Console.WriteLine($"Evicted: Key = {key}, Value = {value}");
            };

            // Adding items to the cache
            Console.WriteLine("Adding items to the cache...");
            cache.Add("A", 1);
            cache.Add("B", 2);
            cache.Add("C", 3);
            Console.WriteLine("Cache filled.");

            // Retrieving an item (this will move "B" to the front as the most recently used)
            Console.WriteLine("Retrieving item with key 'B': " + cache.Get("B"));

            // Adding another item (this will cause an eviction since the cache is at capacity)
            Console.WriteLine("Adding item with key 'D', value 4...");
            cache.Add("D", 4);

            // Attempting to retrieve an evicted item (this will throw a KeyNotFoundException)
            Console.WriteLine("Trying to retrieve evicted item with key 'A'...");
            try
            {
                Console.WriteLine(cache.Get("A"));
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Item with key 'A' not found!");
            }

            // Removing an item from the cache
            Console.WriteLine("Removing item with key 'C'...");
            if (cache.TryRemove("C"))
            {
                Console.WriteLine("Item with key 'C' removed successfully.");
            }
            else
            {
                Console.WriteLine("Item with key 'C' not found!");
            }

            // Final state of the cache
            Console.WriteLine("Final state of the cache:");
            Console.WriteLine("Key 'B': " + cache.Get("B"));
            Console.WriteLine("Key 'D': " + cache.Get("D"));

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
