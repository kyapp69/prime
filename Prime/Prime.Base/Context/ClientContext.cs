﻿using System;
using System.IO;
using GalaSoft.MvvmLight.Messaging;

namespace Prime.Core
{
    /// <summary>
    /// Singleton, one per application.
    /// </summary>
    public class ClientContext : IPrimeContext
    {
        public readonly IMessenger M;
        public readonly PrimeClientConfig Config;

        public ClientContext() : this("[src]/instance/prime-client.config") { } //used for testing / debug

        public ClientContext(string configPath) : this(configPath, DefaultMessenger.I.Instance) { }

        public ClientContext(string configPath, IMessenger m)
        {
            configPath = configPath.ResolveSpecial();

            _testing = this; //todo: hack for now.

            if (string.IsNullOrWhiteSpace(configPath))
                throw new ArgumentException($"\'{nameof(configPath)}\' cannot be empty.");

            Config = PrimeClientConfig.Get(Path.GetFullPath(configPath));
            M = m;
        }

        private static ClientContext _testing = new ClientContext();
        public static ClientContext Testing => _testing ?? new ClientContext();

        private DirectoryInfo _appDataDirectoryInfo;
        public DirectoryInfo AppDataDirectoryInfo => _appDataDirectoryInfo ?? (_appDataDirectoryInfo = GetAppBase());

        private ILogger _logger;
        public ILogger L
        {
            get => _logger ?? (_logger = new MessengerLogger(M));
            set => _logger = value;
        }

        private ClientFileSystem _clientFs;
        public ClientFileSystem FileSystem => _clientFs ?? (_clientFs = new ClientFileSystem(this));

        private DirectoryInfo GetAppBase()
        {
            if (!string.IsNullOrWhiteSpace(Config.BasePath))
                return new DirectoryInfo(Path.GetFullPath(Config.BasePath));

            if (Config.ConfigLoadedFrom != null)
                return Config.ConfigLoadedFrom.Directory;

            return new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }
    }
}