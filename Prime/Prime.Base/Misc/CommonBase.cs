﻿using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight.Messaging;
using Prime.Core;

namespace Prime.Base
{
    public abstract class CommonBase
    {
        protected readonly PrimeContext C;
        public ILogger L => C.L;
        public IMessenger M => C.M;

        protected CommonBase(PrimeContext context)
        {
            C = context;
        }
    }
}
