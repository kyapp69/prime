﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using LiteDB;
using Prime.Base;
using Prime.Core;

namespace Prime.Finance
{
    [Export(typeof(IExtension))]
    public class FinanceServer : IExtensionExecute
    {
        private static readonly ObjectId _id = "prime:finance".GetObjectIdHashCode();
        public ObjectId Id => _id;

        public string Title { get; } = "Prime Finance";

        public Version Version { get; } = new Version("1.0.0");

        public void Main(PrimeContext context)
        {
            //
        }
    }
}
