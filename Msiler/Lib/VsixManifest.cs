using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Msiler.Lib
{
    public class VsixManifest
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Id { get; }

        public string Version { get; }

        private VsixManifest(string manifestPath)
        {
            var doc = new XmlDocument();
            doc.Load(manifestPath);

            if (doc.DocumentElement == null || doc.DocumentElement.Name != "PackageManifest")
                return;

            var metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Metadata");
            var identity = metaData.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Identity");

            this.Id = identity.GetAttribute("Id");
            this.Version = identity.GetAttribute("Version");
        }

        public static VsixManifest GetManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyUri = new UriBuilder(assembly.CodeBase);

            string assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);

            if (assemblyDirectory == null)
                throw new InvalidOperationException();

            string vsixManifestPath = Path.Combine(assemblyDirectory, "extension.vsixmanifest");
            return new VsixManifest(vsixManifestPath);
        }
    }
}
