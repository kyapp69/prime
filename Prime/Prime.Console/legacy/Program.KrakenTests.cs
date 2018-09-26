﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Prime.Core;
using Prime.Finance;
using Prime.Finance.Services.Services.Kraken;

namespace Prime.TestConsole
{
    public partial class Program
    {
        public class KrakenTests
        {
            public void GetDepositAddresses()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();

                var ctx = new WalletAddressAssetContext("BTC".ToAsset(provider), UserContext.Testing);

                try
                {
                    var addresses = AsyncContext.Run(() => provider.GetAddressesForAssetAsync(ctx));

                    foreach (var walletAddress in addresses.WalletAddresses)
                    {
                        System.Console.WriteLine($"{walletAddress.Asset} : {walletAddress.Address}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetLatestPrice()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();
                var ctx = new PublicPriceContext(new AssetPair("BTC", "USD"));

                try
                {
                    var price = AsyncContext.Run(() => provider.GetPricingAsync(ctx));

                    System.Console.WriteLine($"Latest {ctx.Pair} value is {price.FirstPrice.Price}");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetFundingMethod()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();

                var ctx = new NetworkProviderPrivateContext(UserContext.Testing);

                try
                {
                    var method = AsyncContext.Run(() => provider.GetFundingMethodAsync(ctx, Asset.Btc));

                    System.Console.WriteLine($"Funding method: {method}");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    // throw;
                }
            }

            public void GetAssetPairs()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();
                var ctx = new NetworkProviderPrivateContext(UserContext.Testing);

                var pairs = AsyncContext.Run(() => provider.GetAssetPairsAsync(ctx));

                try
                {
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
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();
                var ctx = new NetworkProviderPrivateContext(UserContext.Testing);

                try
                {
                    var balances = AsyncContext.Run(() => provider.GetBalancesAsync(ctx));

                    if (balances.Count == 0)
                    {
                        System.Console.WriteLine("No balances.");
                    }
                    else
                    {
                        foreach (var balance in balances)
                        {
                            System.Console.WriteLine(
                                $"{balance.Asset}: {balance.Available}, {balance.AvailableAndReserved}, {balance.Reserved}");
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void GetOhlc()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();

                var ctx = new OhlcContext(new AssetPair("BTC", "USD"), TimeResolution.Minute, null);

                try
                {
                    var ohlc = AsyncContext.Run(() => provider.GetOhlcAsync(ctx));

                    foreach (var data in ohlc.OhlcData)
                    {
                        System.Console.WriteLine($"{data.DateTimeUtc}: {data.High} {data.Low}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }
            }

            public void TestApi()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();

                var ctx = new ApiPrivateTestContext(UserContext.Testing.GetApiKey(provider));

                try
                {
                    var result = AsyncContext.Run(() => provider.TestPrivateApiAsync(ctx));

                    System.Console.WriteLine($"Api test ok: {result}");
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    throw;
                }

            }

            public void GetAllAddressesAsync()
            {
                var provider = Networks.I.Providers.OfType<KrakenProvider>().FirstProvider();

                var ctx = new WalletAddressContext(UserContext.Testing);

                try
                {
                    var addresses = AsyncContext.Run(() => provider.GetAddressesAsync(ctx));

                    foreach (var walletAddress in addresses.WalletAddresses)
                    {
                        System.Console.WriteLine($"{walletAddress.Asset} : {walletAddress.Address}");
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    // throw;
                }
            }
        }
    }
}
