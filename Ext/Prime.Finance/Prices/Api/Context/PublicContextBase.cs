﻿using System.Collections.Generic;
using System.Linq;
using Prime.Core;

namespace Prime.Finance
{
    public abstract class PublicContextBase : NetworkProviderContext
    {
        public IList<AssetPair> Pairs { get; protected set; }

        protected PublicContextBase(ILogger logger = null) : base(logger)
        {
            IsRequestAll = true;
            Pairs = new List<AssetPair>();
        }

        protected PublicContextBase(IList<AssetPair> pairs, ILogger logger = null) : base(logger)
        {
            Pairs = pairs;
        }

        public AssetPair Pair => Pairs.FirstOrDefault();

        public virtual bool UseBulkContext => true;

        public bool IsMultiple => IsRequestAll || Pairs.Count > 1;

        public bool ForSingleMethod => !UseBulkContext;

        public bool IsRequestAll { get; private set; }
    }
}