using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace Xrm.WebResource.Deployer
{
    public class PackageGenerator
    {
        private GenerationArgs Args { get; }
        private readonly ILog log;

        public PackageGenerator(GenerationArgs args, ILog log)
        {
            Args = args;
            this.log = log;
        }

        public void GeneratePackagesXml()
        {
            var preFix = Args.DeployPrefix;
            var rootPath = Args.SourceFolder;
            var allFiles = DirSearch(rootPath);
            var sortedList = allFiles.OrderBy(x => x).ToList();

            var allFolderPaths = Directory.GetDirectories(rootPath).ToList();

            var packages = allFolderPaths.Select(folderPath =>
            {
                return new PackageInfo
                {
                    packageName = folderPath.Replace(rootPath, "").Replace("\\", ""),
                    prefix = preFix,
                    rootPath = rootPath,
                    webResourceInfos = sortedList.Where(file => file.StartsWith(folderPath)).Select(listElement => new WebResourceInfo
                    {
                        extension = Path.GetExtension(listElement).Replace(".", ""),
                        fileName = listElement.Replace($"{folderPath}\\", ""),
                        displayName = Path.GetFileName(listElement),
                        relativeFilePath = listElement.Replace(rootPath, "")

                    }).ToList()
                };
            }).ToList();


            var completeString = FillPackages(packages);

            File.WriteAllText(@".\packages.xml", completeString);

            log.Info("Done.");
        }



        private List<String> DirSearch(string rootPath)
        {
            List<string> files = new List<string>();


            try
            {
                foreach (string f in Directory.GetFiles(rootPath))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(rootPath))
                {

                    files.AddRange(DirSearch(d));
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            return files;
        }

        private string FillWebResourceInfo(WebResourceInfo webresourceInfo)
        {
            var fullFileName = $"{ webresourceInfo.fileName.Replace("\\", "/") }";
            var webresourceInfoString = $"<WebResourceInfo name='{fullFileName}' filePath='{webresourceInfo.relativeFilePath}' displayName='{webresourceInfo.displayName}' type='{webresourceInfo.extension}' description=''/>";
            return webresourceInfoString;
        }

        private string FillPackage(PackageInfo packageInfo)
        {
            var package = $"<Package name='{packageInfo.prefix}/{packageInfo.packageName}/' rootPath='{packageInfo.rootPath}' isNamePrefix='true'>" + "\n\t\t\t" +
            $"{ string.Join("\n\t\t\t", packageInfo.webResourceInfos.Select(FillWebResourceInfo)) }" + "\n\t\t" + "</Package>";
            return package;
        }

        private string FillPackages(List<PackageInfo> packageInfos)
        {
            var packagesString =
                "<?xml version='1.0' encoding='utf-8'?>" + "\n" + "<UtilityRoot>" + "\n\t" + "<Packages>" + "\n\t\t" + $"{string.Join("\n\t\t", packageInfos.Select(FillPackage))}" + "\n\t" + "</Packages>" + "\n</UtilityRoot>";

            return packagesString;
        }
    }

    public class WebResourceInfo
    {
        public string fileName;
        public string extension;
        public string relativeFilePath;
        public string displayName;
    }

    public class PackageInfo
    {
        public string packageName;
        public List<WebResourceInfo> webResourceInfos;
        public string rootPath;
        public string prefix;
    }
}
