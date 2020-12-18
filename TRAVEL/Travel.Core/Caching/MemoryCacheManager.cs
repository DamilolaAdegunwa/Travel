using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
namespace Travel.Core.Caching
{
    public partial class MemoryCacheManager : ICacheManager
    {
        public IMemoryCache Cache { get; private set; }

        public MemoryCacheManager()
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
        }
        //public virtual void Clear()
        //{
        //    Cache..CreateEntry(new object())..Dispose();
        //    foreach (var item in Cache)
        //        Remove(item.Key);
        //}

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public virtual T Get<T>(string key)
        {
            //throw new NotImplementedException();
            return (T)Cache.Get(key);
        }

        public bool IsSet(string key)
        {
            //throw new NotImplementedException();
            return (Cache.Get(key) != null);
        }

        public void Remove(string key)
        {
            //throw new NotImplementedException();
            Cache.Remove(key);
        }

        //public void RemoveByPattern(string pattern)
        //{
        //    //throw new NotImplementedException();
        //    //this.RemoveByPattern(pattern, Cache.Select(p => p.Key));
        //}

        public void Set(string key, object data, int cacheTime)
        {
            //throw new NotImplementedException();
            if(data == null)
            {
                return;
            }
            Cache.Set(key, data, DateTime.Now + TimeSpan.FromMinutes(cacheTime));
        }
    }
}
