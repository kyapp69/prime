﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prime.Base;
using Prime.Core;

namespace Prime.Core
{
    /// <summary>
    /// Singleton, one per application.
    /// </summary>
    public class PublicContext : IDataContext
    {
        public static string Version = "v0.02";
        public readonly PrimeContext PrimeContext;

        public PublicContext(PrimeContext primeContext)
        {
            PrimeContext = primeContext;
            Id = "prime:public:context".GetObjectIdHashCode();
            I = this;
        }

        public static PublicContext I;

        public readonly ObjectId Id;

        ObjectId IDataContext.Id => Id;

        DirectoryInfo IDataContext.StorageDirectory => StorageDirectoryPub;

        public bool IsPublic => true;
        
        private DirectoryInfo _storageDirectory;
        public DirectoryInfo StorageDirectoryPub => _storageDirectory ?? (_storageDirectory = PrimeContext.FileSystem.PrimeWorkspaceDirectory.EnsureSubDirectory("pub"));
        
        /*
        private ExchangeDatas _exchangeData;
        public ExchangeDatas ExchangeDatas => _exchangeData ?? (_exchangeData = new ExchangeDatas());
        */

        private readonly object _singletonLock = new object();
        private readonly List<object> _singletons = new List<object>();

        public T GetOrAddSingleton<T>(Func<T> create)
        {
            lock (_singletonLock)
            {
                var vm = _singletons.OfType<T>().FirstOrDefault();
                if (vm != null)
                    return vm;

                vm = create.Invoke();
                _singletons.Add(vm);
                return vm;
            }
        }
    }
}
