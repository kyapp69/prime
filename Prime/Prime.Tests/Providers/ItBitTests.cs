﻿using System.Collections.Generic;
using System.Linq;
using Prime.Core; using Prime.Finance;
using Prime.Finance.Services.Services.ItBit;
using Xunit;
using Xunit.Abstractions;

namespace Prime.Tests.Providers
{
    public class ItBitTests : ProviderDirectTestsBase
    {
        public ItBitTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            Provider = Networks.I.Providers.OfType<ItBitProvider>().FirstProvider();
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
                "BTC_SGD".ToAssetPairRaw(),
                "BTC_EUR".ToAssetPairRaw(),
            };

            base.PretestGetPricing(pairs, false);
        }

        [Fact]
        public override void TestGetAssetPairs()
        {
            var requiredPairs = new AssetPairs(new List<AssetPair>()
            {
                "BTC_USD".ToAssetPairRaw(),
                "BTC_SGD".ToAssetPairRaw(),
                "BTC_EUR".ToAssetPairRaw(),
            });

            base.PretestGetAssetPairs(requiredPairs);
        }
    }
}
