﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LiteDB;
using Prime.Base;
using Prime.Core;
using RestEase;

namespace Prime.Finance.Services.Services.Korbit
{
    // https://apidocs.korbit.co.kr/
    /// <author email="yasko.alexander@gmail.com">Alexander Yasko</author>
    public class KorbitProvider : IOrderBookProvider, IPublicPricingProvider, IAssetPairsProvider
    {
        public Version Version { get; } = new Version(1, 0, 0);
        private static readonly ObjectId IdHash = "prime:korbit".GetObjectIdHashCode();
        private static readonly string _pairs = "btckrw,etckrw,ethkrw,xrpkrw";
        
        public const string KorbitApiVersion = "v1/";
        public const string KorbitApiUrl = "https://api.korbit.co.kr/" + KorbitApiVersion;

        private RestApiClientProvider<IKorbitApi> ApiProvider { get; }

        public AssetPairs Pairs => new AssetPairs(3, _pairs, this);
        public ObjectId Id => IdHash;
        public Network Network { get; } = Networks.I.Get("Korbit");
        public bool Disabled => false;
        public int Priority => 100;
        public string AggregatorName => null;
        public string Title => Network.Name;
        public bool IsDirect => true;
        public char? CommonPairSeparator { get; }

        // https://apidocs.korbit.co.kr/#first_section
        // ... Ticker calls are limited to 60 calls per 60 seconds. ...
        public IRateLimiter RateLimiter { get; } = new PerMinuteRateLimiter(60, 1);
        public ApiConfiguration GetApiConfiguration => ApiConfiguration.Standard2;

        public bool CanGenerateDepositAddress => false;
        public bool CanPeekDepositAddress => false;

        public KorbitProvider()
        {
            ApiProvider = new RestApiClientProvider<IKorbitApi>(KorbitApiUrl, this, k => new KorbitAuthenticator(k).GetRequestModifierAsync);
        }

        public async Task<bool> TestPublicApiAsync(NetworkProviderContext context)
        {
            var ctx = new PublicPriceContext("BTC_KRW".ToAssetPairRaw());

            var r = await GetPricingAsync(ctx).ConfigureAwait(false);

            return r != null;
        }

        private static readonly PricingFeatures StaticPricingFeatures = new PricingFeatures()
        {
            Single = new PricingSingleFeatures() { CanStatistics = true, CanVolume = true }
        };
        public PricingFeatures PricingFeatures => StaticPricingFeatures;

        public async Task<MarketPrices> GetPricingAsync(PublicPricesContext context)
        {
            var api = ApiProvider.GetApi(context);
            var pairCode = context.Pair.ToTicker(this, '_').ToLower();

            try
            {
                var r = await api.GetDetailedTickerAsync(pairCode).ConfigureAwait(false);

                var sTimeStamp = r.timestamp / 1000; // r.timestamp is returned in ms.

                var price = new MarketPrice(Network, context.Pair, r.last, sTimeStamp.ToUtcDateTime())
                {
                    PriceStatistics = new PriceStatistics(Network, context.Pair.Asset2, r.ask, r.bid, r.low, r.high),
                    Volume = new NetworkPairVolume(Network, context.Pair, r.volume)
                };

                return new MarketPrices(price);
            }
            catch (ApiException ex)
            {
                if(ex.StatusCode == HttpStatusCode.BadRequest)
                    throw new AssetPairNotSupportedException(context.Pair, this);
                throw;
            }
        }

        public Task<AssetPairs> GetAssetPairsAsync(NetworkProviderContext context)
        {
            return Task.Run(() => Pairs);
        }

        public async Task<OrderBook> GetOrderBookAsync(OrderBookContext context)
        {
            var api = ApiProvider.GetApi(context);
            var pairCode = context.Pair.ToTicker(this, '_').ToLower();

            var r = await api.GetOrderBookAsync(pairCode).ConfigureAwait(false);

            var bids = context.MaxRecordsCount == Int32.MaxValue 
                ? r.bids
                : r.bids.Take(context.MaxRecordsCount) ;

            var asks = context.MaxRecordsCount == Int32.MaxValue
                ? r.asks
                : r.asks.Take(context.MaxRecordsCount);

            var orderBook = new OrderBook(Network, context.Pair);

            foreach (var i in bids)
                orderBook.AddBid(i[0], i[1]);

            foreach (var i in asks)
                orderBook.AddAsk(i[0], i[1]);

            return orderBook;
        }

        public IAssetCodeConverter GetAssetCodeConverter()
        {
            return null;
        }
    }
}
