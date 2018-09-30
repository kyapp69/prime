﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Prime.Core;
using Prime.Finance;
using Prime.Finance.Services.Services.Bittrex;

namespace Prime.TestConsole
{
	public partial class Program
	{
		public class BittrexTests
		{
			public void ApiTest()
			{
				var provider = Networks.I.Providers.OfType<BittrexProvider>().FirstProvider();
				var apiTestCtx = new ApiPrivateTestContext(UserContext.Testing.GetApiKey(provider));

				var ok = AsyncContext.Run(() => provider.TestPrivateApiAsync(apiTestCtx));


				try
				{

					System.Console.WriteLine($"Api test OK: {ok}");
				}
				catch (Exception e)
				{
					System.Console.WriteLine(e.Message);
					throw;
				}
			}

			public void GetDepositAddresses()
			{
				var provider = Networks.I.Providers.OfType<BittrexProvider>().FirstProvider();

				var ctx = new WalletAddressAssetContext("BTC".ToAsset(provider), UserContext.Testing);
				var ctxAll = new WalletAddressContext(UserContext.Testing);

				var addresses = AsyncContext.Run(() => provider.GetAddressesForAssetAsync(ctx));
				var addressesAll = AsyncContext.Run(() => provider.GetAddressesAsync(ctxAll));

				try
				{
					System.Console.WriteLine("Addresses for 1 asset");
					foreach (var address in addresses.WalletAddresses)
					{
						System.Console.WriteLine($"{address.Asset} : {address.Address}");
					}

					System.Console.WriteLine("Addresses for all assets");
					foreach (var address in addressesAll.WalletAddresses)
					{
						System.Console.WriteLine($"{address.Asset} : {address.Address}");
					}
				}
				catch (Exception e)
				{
					System.Console.WriteLine(e);
					throw;
				}
			}

			public void GetAssetPairs()
			{
				var provider = Networks.I.Providers.OfType<BittrexProvider>().FirstProvider();
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
				var provider = Networks.I.Providers.OfType<BittrexProvider>().FirstProvider();
				var ctx = new NetworkProviderPrivateContext(UserContext.Testing);

				var balances = AsyncContext.Run(() => provider.GetBalancesAsync(ctx));

				try
				{


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

			public void LatestPrices()
			{
				var provider = Networks.I.Providers.OfType<BittrexProvider>().FirstProvider();
				var pair = new AssetPair("BTC", "LTC");

				var ctx = new PublicPriceContext(pair);

				try
				{
					var price = AsyncContext.Run(() => provider.GetPriceAsync(ctx));

					System.Console.WriteLine($"Latest price for {pair} is {price.FirstPrice}");
				}
				catch (Exception e)
				{
					System.Console.WriteLine(e);
					throw;
				}
			}
		}
	}
}
