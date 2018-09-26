﻿using System.IO;
using Prime.Core;

namespace Prime.IPFS
{
    public class IpfsInstanceContext
    {
        public readonly PrimeContext PrimeContext;

        public IpfsInstanceContext(PrimeContext context, IpfsPlatformBase platform)
        {
            PrimeContext = context;
            WorkspaceDirectory = context.FileSystem.GetExtWorkspace(platform.Instance);
            Platform = platform;
            L = context.L ?? new NullLogger();
        }

        public DirectoryInfo WorkspaceDirectory { get; private set; }

        public IpfsPlatformBase Platform { get; private set; }

        public ILogger L { get; set; }
    }
}