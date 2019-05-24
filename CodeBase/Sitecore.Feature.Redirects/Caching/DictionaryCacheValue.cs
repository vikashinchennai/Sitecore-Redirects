using Sitecore.Caching;
using Sitecore.Data;
using Sitecore.Reflection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
namespace Sitecore.Geek.Feature.Redirects.Caching
{
    public class DictionaryCacheValue : ICacheable
    {
        public bool Cacheable
        {
            get;
            set;
        }

        public bool Immutable
        {
            get
            {
                return true;
            }
        }

        public Dictionary<object, object> Properties
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public DictionaryCacheValue()
        {
            this.Properties = new Dictionary<object, object>();
        }

        public long GetDataLength()
        {
            if (this.Value == null)
            {
                return (long)TypeUtil.SizeOfObject();
            }
            return (long)TypeUtil.SizeOfString(this.Value);
        }

        public ID GetValueAsId(string id)
        {
            return new ID(this.Properties[id].ToString());
        }

        public event DataLengthChangedDelegate DataLengthChanged;
    }
}
