using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Xrm.WebResource.Deployer.Publisher
{
    public static class FileLoader
    {
        /// <summary>
        /// Loads all packages of given package.xml
        /// </summary>
        /// <param name="packagesFilename"></param>
        /// <returns></returns>
        public static ObservableCollection<XElement> LoadPackages(string packagesFilename)
        {
            var xmlPackagesDocument = XDocument.Load(packagesFilename);
            var xmlPackageData = xmlPackagesDocument.Element(PackageInfo.UtilityRoot);
            var packageCollection = new ObservableCollection<XElement>();
            var packages = xmlPackageData?.Element(PackageInfo.Packages)?.Descendants(PackageInfo.Package);

            packages = packages ?? throw new InvalidOperationException("No packages defined in related package.xml");

            foreach (var p in packages)
            {
                packageCollection.Add(p);
            }

            return packageCollection;
        }
    }
}
