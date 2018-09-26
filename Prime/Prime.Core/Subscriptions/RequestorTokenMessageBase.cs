﻿namespace Prime.Core.Messages
{
    public abstract class RequestorTokenMessageBase
    {
        public readonly string RequesterToken;

        protected RequestorTokenMessageBase(string requesterToken)
        {
            RequesterToken = requesterToken;
        }
    }
}