﻿using System;
using System.Threading.Tasks;
using Prime.Base;
using Prime.Core;

namespace Prime.Core
{
    public interface INetworkProvider : IExtension
    {
        Network Network { get; }

        bool Disabled { get; }

        int Priority { get; }

        string AggregatorName { get; }

        IRateLimiter RateLimiter { get; }

        /// <summary>
        /// This is not a proxy or aggregator service, it connects directly to the service specified
        /// </summary>
        bool IsDirect { get; }

        // TODO: AY: change bool to PublicApiResponse.
        Task<bool> TestPublicApiAsync(NetworkProviderContext context);
    }
}