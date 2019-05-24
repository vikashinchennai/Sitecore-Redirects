using Sitecore.Caching;
using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Geek.Feature.Redirects.Caching
{
    public class DictionaryCache : CustomCache
    {
        public DictionaryCache(string name, long maxSize) : base(name, maxSize)
        {
        }

        public virtual DictionaryCacheValue Get(ID id)
        {
            return base.GetObject(id.ToString()) as DictionaryCacheValue;
        }

        public virtual DictionaryCacheValue Get(string id)
        {
            return base.GetObject(id) as DictionaryCacheValue;
        }

        public virtual void Set(ID id, DictionaryCacheValue value)
        {
            base.SetObject(id.ToString(), (ICacheable)value);
        }

        public virtual void Set(string id, DictionaryCacheValue value)
        {
            base.SetObject(id, (ICacheable)value);
        }
    }
}
