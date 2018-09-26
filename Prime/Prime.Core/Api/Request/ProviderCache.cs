using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Prime.Core;

namespace Prime.Core
{
    public abstract class ProviderCache<T,T2> : CacheDictionary<T, T2>
    {
        protected ProviderCache(TimeSpan expirationSpan) : base(expirationSpan)
        {
        }

        public Task<T2> TryAsync(T provider, Func<Task<T2>> pull)
        {
            return Task.Run(() =>
            {
                return Try(provider, k => AsyncContext.Run(pull.Invoke));
            });
        }
    }
}