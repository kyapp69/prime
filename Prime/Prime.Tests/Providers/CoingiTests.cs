﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prime.Core;
using Prime.Finance;
using Prime.Finance.Services.Services.Coingi;
using Xunit;
using Xunit.Abstractions;

namespace Prime.Tests.Providers
{
    public class CoingiTests : ProviderDirectTestsBase
    {
        public CoingiTests(ITestOutputHelper outputWriter) : base(outputWriter)
        {
            Provider = Networks.I.Providers.OfType<CoingiProvider>().FirstProvider();
        }

        [Fact]
        public override void TestApiPublic()
        {
            base.TestApiPublic();
        }

        [Fact]
        public override void TestApiPrivate()
        {
            base.TestApiPrivate();
        }

        [Fact]
        public override void TestGetOrderBook()
        {
            base.PretestGetOrderBook("BTC_USD".ToAssetPairRaw(), false);
        }

        [Fact]
        public override void TestGetTradeOrderStatus()
        {
            // TODO: SC: Not tested with real money
            var orderId = "21109502";
            base.PretestGetTradeOrderStatus(orderId, "BTC_USD".ToAssetPairRaw());
        }

        [Fact]
        public override void TestPlaceOrderLimit()
        {
            //TODO: SC: Not tested with real money

            base.PretestPlaceOrderLimit("BTC_USD".ToAssetPairRaw(), true, new Money(10, Asset.Usd), new Money(10m, Asset.Usd));
        }


        [Fact]
        public override void TestPlaceWithdrawal()
        {
            // TODO: SC: Not tested with real money
            base.PretestPlaceWithdrawal(new WalletAddress("1234"), new Money(22, Asset.Btc));
        }
    }
}
