using Finbourne.InMemoryCache.Implementations;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

namespace Finbourne.InMemoryCache.Tests
{
    public class CacheTests
    {
        private readonly Cache<string, int> _cache;

        public CacheTests()
        {
            _cache = Cache<string, int>.GetInstance(100);
        }

        [Fact]
        public void TestAddAndGet()
        {
            _cache.Add("Maria", 1);
            _cache.Add("Saulo", 2);
            _cache.Add("candido", 3);

            Assert.Equal(1, _cache.Get("Maria"));
            Assert.Equal(2, _cache.Get("Saulo"));
            Assert.Equal(3, _cache.Get("candido"));
        }

        [Fact]
        public void TestUpdateExistingItem()
        {
            _cache.Add("a", 1);
            _cache.Add("a", 100);

            Assert.Equal(100, _cache.Get("a"));
        }

        [Fact]
        public void TestKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => _cache.Get("non_existent_key"));
        }

        [Fact]
        public void TestTryRemove()
        {
            _cache.Add("a", 1);

            Assert.True(_cache.TryRemove("a"));
            Assert.False(_cache.TryRemove("non_existent_key"));
        }

        [Fact]
        public void TestSingletonBehavior()
        {
            var cache1 = Cache<string, int>.GetInstance(100);
            var cache2 = Cache<string, int>.GetInstance(100);

            Assert.Equal(cache1, cache2);
        }

        [Fact]
        public async Task TestConcurrentGetAndAdd()
        {
            int capacity = 100;

            // Create a few threads that will concurrently add and get items from the cache
            var tasks = Enumerable.Range(0, 10).Select(x =>
            {
                return Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        _cache.Add(i.ToString(), i);
                        _cache.Get(i.ToString());
                    }
                });
            }).ToArray();

            // Wait for all of the threads to finish
            await Task.WhenAll(tasks);

            // Verify that all of the items are in the cache
            Assert.Equal(capacity, _cache.Count); // The count should match the capacity, not the total number of additions
        }
    }
}

