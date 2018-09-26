﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Prime.Core;
using Prime.Finance;
using Prime.Finance.Services.Services.BitMex;
using Prime.Finance.Wallet;

namespace Prime.TestConsole
{
    public partial class Program
    {
        public class BitMexTests
        {
            public void TestApi()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();

                var ctx = new ApiPrivateTestContext(UserContext.Testing.GetApiKey(provider));
  
                try
                {
                    var r = AsyncContext.Run(() => provider.TestPrivateApiAsync(ctx));

                    System.Console.WriteLine($"Api success: {r}");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }

            public void TestPortfolioAccountBalances()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();

                var c = new PortfolioProviderContext(UserContext.Testing, provider, UserContext.Testing.Finance().QuoteAsset, 0);
                var scanner = new PortfolioProvider(c);
                try
                {
                    scanner.Update();
                    foreach (var i in scanner.Items)
                        System.Console.WriteLine(i.Asset.ShortCode + " " + i.AvailableBalance);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }

            public void GetDepositAddresses()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();

                var asset = "BTC".ToAsset(provider);

                var ctx = new WalletAddressAssetContext(asset, UserContext.Testing);

                try
                {
                    var r = provider.GetAddressesForAssetAsync(ctx).Result;

                    System.Console.WriteLine("List of addresses: ");
                    foreach (var walletAddress in r.WalletAddresses)
                    {
                        System.Console.WriteLine($"{walletAddress.Asset}: {walletAddress.Address}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                    throw;
                }
            }

            public void GetOhlcData()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();

                var ohlcContext = new OhlcContext(new AssetPair("BTC", "USD"), TimeResolution.Minute, new TimeRange(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, TimeResolution.Minute));

                try
                {
                    var ohlc = AsyncContext.Run(() => provider.GetOhlcAsync(ohlcContext));

                    foreach (var data in ohlc.OhlcData)
                    {
                        System.Console.WriteLine($"{data.DateTimeUtc}: {data.High} {data.Low} {data.Open} {data.Close}");
                    }

                    System.Console.WriteLine($"Entries count: {ohlc.OhlcData.Count}");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetLatestPrice()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();

                var ctx = new PublicPriceContext(new AssetPair("USD".ToAsset(provider), "BTC".ToAssetRaw()));

                try
                {
                    var c = AsyncContext.Run(() => provider.GetPriceAsync(ctx));

                    System.Console.WriteLine($"Base asset: {ctx.QuoteAsset}\n");
                    System.Console.WriteLine(c.FirstPrice.Price.Display);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetAssetPairs()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();
                var ctx = new NetworkProviderContext();

                try
                {
                    var pairs = AsyncContext.Run(() => provider.GetAssetPairsAsync(ctx));

                    foreach (var pair in pairs)
                    {
                        System.Console.WriteLine($"{pair}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetBalances()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();
                var ctx = new NetworkProviderPrivateContext(UserContext.Testing);

                try
                {
                    var balances = AsyncContext.Run(() => provider.GetBalancesAsync(ctx));

                    foreach (var balance in balances)
                    {
                        System.Console.WriteLine($"{balance.Asset} : {balance.AvailableAndReserved}, {balance.Available}, {balance.Reserved}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetAllDepositAddresses()
            {
                var provider = Networks.I.Providers.OfType<BitMexProvider>().FirstProvider();
                
                var ctx = new WalletAddressContext(UserContext.Testing);

                try
                {
                    var addresses = AsyncContext.Run(() => provider.GetAddressesAsync(ctx));

                    System.Console.WriteLine("All addresses:");

                    foreach (var address in addresses.WalletAddresses)
                    {
                        System.Console.WriteLine($"{address.Asset}: {address.Address}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }
        }
    }
}
