using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Prime.Core
{
    [XmlRoot("config")]
    public class PrimeClientConfig : XmlConfigBase
    {
        private static string FileName = "prime-client.config";

        public PrimeClientConfig() {}

        public static PrimeClientConfig Get(string path)
        {
            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                if (!TryDefault(fi) || !fi.Exists)
                    throw new Exception(fi.FullName + " does not exist, and no .default file present.");
            }

            var pc = Load<PrimeClientConfig>(new FileInfo(path));
            if (pc != null)
                pc.ConfigLoadedFrom = fi;

            return pc ?? new PrimeClientConfig();
        }

        private static bool TryDefault(FileInfo fi)
        {
            var dfi = new FileInfo(fi.FullName + ".default");
            if (!dfi.Exists)
                return false;

            dfi.CopyTo(fi.FullName, true);
            fi.Refresh();
            return true;
        }

        public void Save(DirectoryInfo directory)
        {
            Save(GetFileInfo(directory, FileName));
        }

        [XmlIgnore]
        public FileInfo ConfigLoadedFrom { get; private set; }


        [XmlElement("fileSystemPath")]
        public string BasePath { get; set; }
        
        [XmlElement("service")]
        public ServiceTypeEnum ServiceType { get; set; }

        [XmlElement("connection")]
        public string ConnectionString { get; set; }

        public bool IsStandalone => ServiceType == ServiceTypeEnum.Standalone;

        [XmlElement("packages")]
        public PackageConfig PackageConfig { get; set; } = new PackageConfig();
    }
}