﻿using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using LiteDB;
using Prime.Base;
using Prime.Core;
using Prime.Finance.Exchange.Trading_temp;
using Prime.Finance.Exchange.Trading_temp.Messages;

namespace Prime.Finance
{
    public class TradingCoordinator
    {
        private readonly IMessenger _m = DefaultMessenger.I.Instance;

        private TradingCoordinator()
        {
            //Strategies = TypeCatalogue.I.ImplementInstances<ITradeStrategy>();
        }

        public static TradingCoordinator I => Lazy.Value;
        private static readonly Lazy<TradingCoordinator> Lazy = new Lazy<TradingCoordinator>(()=>new TradingCoordinator());

        //public IReadOnlyList<ITradeStrategy> Strategies;

        private readonly List<ExecutingTrade> _executing = new List<ExecutingTrade>();

        public void Process(ITradeStrategy strategy)
        {
            _executing.Add(new ExecutingTrade(strategy));
            strategy.Start();
        }

        public void Changed(ObjectId tradeId)
        {
            var strat = _executing.Select(x=>x.Strategy).FirstOrDefault(x => x.Id == tradeId);
            if (strat == null)
                return;

            var status = strat.GetStatus();

            if (status == TradeStrategyStatus.Completed)
                _m.SendAsync(new ResponseTradeMessage(strat));
        }
    }
}