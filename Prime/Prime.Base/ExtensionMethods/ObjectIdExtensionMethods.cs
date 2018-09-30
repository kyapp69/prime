﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Prime.Base;

namespace Prime.Core
{
    public static class ObjectIdExtensionMethods
    {

        /// <summary>
        /// Will generate a reasonably unique consistant objectid from a string.
        /// </summary>
        /// <returns></returns>
        public static ObjectId GetObjectIdHashCode(this string k, bool autoLower = false, bool autoTrim = false)
        {
            if (String.IsNullOrWhiteSpace(k))
                return ObjectId.Empty;

            if (autoLower)
                k = k.ToLower();

            if (autoTrim)
                k = k.Trim();

            using (var algorithm = SHA256.Create())
            {
                // Create the at_hash using the access token returned by CreateAccessTokenAsync.
                var hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(k));
                var bytes = hash.ToArray().Take(12).ToArray();
                return new ObjectId(bytes);
            }
        }

        public static ObjectId GetObjectIdHashCode(this IEnumerable<object> ks, bool autoLower = false, bool autoTrim = false)
        {
            var sb = new StringBuilder(); //because string join changes the order in certain locales?
            foreach (var k in ks)
                sb.Append($"{k}:");
            return GetObjectIdHashCode(sb.ToString(), autoLower, autoTrim);
        }

        private static readonly ConcurrentDictionary<Enum, ObjectId> Obcache = new ConcurrentDictionary<Enum, ObjectId>();
        public static ObjectId GetObjectIdHashCode(this Enum e)
        {
            return Obcache.GetOrAdd(e, @enum => @enum.ToString().ToLower().GetObjectIdHashCode());
        }
    }
}