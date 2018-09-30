﻿using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using LiteDB;
using Prime.Base;
using Prime.Core;
using Prime.Core.Exchange.Rates;
using Prime.Finance.Exchange.Rates;

namespace Prime.Finance.Prices.Latest
{
    internal sealed class Messenger : RenewingSubscriberList<LatestPriceRequestSubscription, SubscriptionRequests>, IStartupMessenger, IDisposable
    {
        public Aggregator Aggregator;

        public void Start(PrimeContext context)
        {
            Aggregator = new Aggregator(this, context);
            M = context.M;
        }

        public void Stop()
        {
            M?.Unregister(this);
            M?.UnregisterAsync(this);
        }

        protected override SubscriptionRequests OnCreatingSubscriber(ObjectId subscriberId, LatestPriceRequestSubscription message)
        {
            lock (Lock)
                return new SubscriptionRequests();
        }

        protected override void OnAddingToSubscriber(MessageListEntry<LatestPriceRequestSubscription, SubscriptionRequests> entry, LatestPriceRequestSubscription message)
        {
            lock (Lock)
            {
                var subs = entry.Subscriber;
                var nr = new Request(message.Pair, message.Network);

                var request = subs.Requests.FirstOrDefault(x => x.Equals(nr));
                if (request != null)
                    return;

                subs.Requests.Add(request = nr);
                request.Processor = new DiscoveryRequestProcessor(request, () =>
                {
                    request.Messenger = new RequestMessenger(request);
                    Aggregator.SyncProviders();
                });
            }
        }

        protected override void OnRemovingFromSubscriber(MessageListEntry<LatestPriceRequestSubscription, SubscriptionRequests> entry, LatestPriceRequestSubscription message)
        {
            lock (Lock)
            {
                var subs = entry.Subscriber;

                var nr = new Request(message.Pair, message.Network);
                var request = subs.Requests.FirstOrDefault(x => x.Equals(nr));
                if (request == null)
                    return;

                request.Dispose();
                subs.Requests.Remove(request);
                Aggregator.SyncProviders();
            }
        }

        protected override void OnRemovingSubscriber(MessageListEntry<LatestPriceRequestSubscription, SubscriptionRequests> entry)
        {
        }

        protected override void OnDisposingSubscription(MessageListEntry<LatestPriceRequestSubscription, SubscriptionRequests> entry)
        {
            //
        }

        public IReadOnlyList<Request> GetRequests()
        {
            lock (Lock)
                return GetSubscribers().SelectMany(x => x.Requests).ToUniqueList();
        }

        public override void Dispose()
        {
            Stop();
        }
    }
}