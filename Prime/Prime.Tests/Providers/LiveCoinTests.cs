﻿using System.Collections.Generic;
using System.Linq;
using Prime.Core; using Prime.Finance;
using Prime.Finance.Services.Services.LiveCoin;
using Xunit;
using Xunit.Abstractions;

namespace Prime.Tests.Providers
{
    public class LiveCoinTests : ProviderDirectTestsBase
    {
        public LiveCoinTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            Provider = Networks.I.Providers.OfType<LiveCoinProvider>().FirstProvider();
        }

        [Fact]
        public override void TestApiPublic()
        {
            base.TestApiPublic();
        }

        [Fact]
        public override void TestGetPricing()
        {
            var pairs = new List<AssetPair>()
            {
                "BTC_USD".ToAssetPairRaw(),
                "LTC_EUR".ToAssetPairRaw(),
                "LTC_BTC".ToAssetPairRaw()
            };

            base.PretestGetPricing(pairs, false);
        }

        [Fact]
        public override void TestGetAssetPairs()
        {
            var requiredPairs = new AssetPairs()
            {
                "BTC_USD".ToAssetPairRaw(),
                "LTC_EUR".ToAssetPairRaw(),
                "LTC_BTC".ToAssetPairRaw()
            };

            base.PretestGetAssetPairs(requiredPairs);
        }

        [Fact]
        public override void TestGetOrderBook()
        {
            base.PretestGetOrderBook("BTC_USD".ToAssetPairRaw(), false);
        }
    }
}
