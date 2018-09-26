﻿using System.Collections.Generic;
using System.Linq;
using Prime.Core;
using Prime.Finance;
using Xunit;

namespace Prime.Tests
{
    public class AssetDiscovery
    {
        [Fact]
        public void TestCompareAggregatorDataWithAssetPairData()
        {
            var pub = FinanceCommon.I.PubData;
            var d = new Dictionary<Network, AssetPairs>();
            foreach (var prov in Networks.I.GetProviders<IAssetPairsProvider>())
            {
                var r = ApiCoordinator.GetAssetPairs(prov);
                if (r.IsFailed)
                    Assert.True(false, prov.Title + " could not perform " + nameof(ApiCoordinator.GetAssetPairs));

                d.Add(prov.Network, r.Response);
            }

            var pairs = d.SelectMany(x => x.Value).ToUniqueList();

            foreach (var pair in pairs)
            {
                var apd = pub.GetAggAssetPairData(pair);
                var aggNets = apd.Exchanges.Select(x => x.Network).ToUniqueList();
                var apNets = d.Where(x => x.Value.Contains(pair)).Select(x => x.Key).ToUniqueList();

                foreach (var n in apNets)
                {
                    if (aggNets.Contains(n))
                        continue;
                    Assert.True(false, $"Aggregated data for {pair} does not contain network: {n.Name}");
                }
            }
        }
    }
}
