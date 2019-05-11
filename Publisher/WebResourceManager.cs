using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xrm.WebResource.Deployer.Entities;
using eWebResource = Xrm.WebResource.Deployer.Entities.WebResource;

namespace Xrm.WebResource.Deployer.Publisher
{
    public class WebResourceManager
    {
        private const string ValidNameMsg = "ERROR: Web Resource names cannot contain spaces or hyphens. They must be alphanumeric and contain underscore characters, periods, and non-consecutive forward slash characters";

        private readonly ObservableCollection<XElement> packages;
        private readonly Crud crud;
        private IOrganizationService Service { get; }
        private CmdArgs Args { get; }
        private readonly bool processAll;
        private readonly ILog err;
        private readonly ILog log;

        public WebResourceManager(IOrganizationService service, ObservableCollection<XElement> packageCollection, Crud crud, ILog log, ILog err, CmdArgs args)
        {
            packages = packageCollection;
            this.crud = crud;
            Service = service;
            Args = args;
            processAll = args.PublishAll;
            this.err = err;
            this.log = log;
        }

        /// <summary>
        /// Manages deployment of webresources
        /// </summary>
        /// <param name="crmResources"></param>
        public void Manage(IEnumerable<eWebResource> crmResources)
        {
            var publishAll = Args.PublishAll;
            var webResources = crmResources as IList<eWebResource> ?? crmResources.ToList();

            List<Guid> webRessourceIds = new List<Guid>();

            int i = 1;
            bool found = false;
            bool allFound = false;
            foreach (var package in packages)
            {
                if (!publishAll && found || !publishAll && allFound)
                {
                    break;
                }

                string packageName = package?.Attribute(PackageInfo.Name)?.Value;
                string rootPath = package?.Attribute(PackageInfo.RootPath)?.Value;

                foreach (var descendant in package?.Descendants()?.ToList())
                {
                    Guid? webRessourceId;
                    if (publishAll)
                    {
                        webRessourceId = Deploy(descendant, packageName, i, webResources, rootPath);
                        if (webRessourceId != null)
                        {
                            webRessourceIds.Add(webRessourceId.Value);
                        }

                        i++;
                    }
                    else if (Args.FileNames != null && Args.FileNames.Length > 0)
                    {
                        if (webRessourceIds.Count == Args.FileNames.Length)
                        {
                            allFound = true;
                            break;
                        }

                        foreach (var fileName in Args.FileNames)
                        {
                            var dName = descendant?.Attribute(WebResource.FilePath)?.Value;

                            if (dName != fileName)
                            {
                                continue;
                            }

                            webRessourceId = Deploy(descendant, packageName, i, webResources, rootPath);
                            if (webRessourceId == null)
                            {
                                continue;
                            }

                            webRessourceIds.Add(webRessourceId.Value);
                            i++;
                            break;
                        }

                    }
                    else if (!string.IsNullOrEmpty(Args.FileName))
                    {
                        if (found)
                        {
                            break;
                        }

                        var dName = descendant?.Attribute(WebResource.FilePath)?.Value;
                        if (dName != Args.FileName)
                        {
                            continue;
                        }

                        webRessourceId = Deploy(descendant, packageName, i, webResources, rootPath);
                        if (webRessourceId == null)
                        {
                            continue;
                        }

                        webRessourceIds.Add(webRessourceId.Value);
                        found = true;
                        break;
                    }
                }

                if (!publishAll && found || !publishAll && allFound)
                {
                    break;
                }
            }

            if (!publishAll && !found && !allFound)
            {
                Console.WriteLine("The file path(s) were not found in Package.xml or a publish of all web resources was not triggered!");
                Environment.Exit(1);
            }

            if (publishAll || found || allFound)
            {
                Console.WriteLine($"Publishing {webRessourceIds.Count} WebRessource(s)...");
                var publisher = new Publisher(Service);
                publisher.Publish(webRessourceIds);
                Console.WriteLine("Done.");
            }
        }

        private Guid? Deploy(XElement descendant, string packageName, int i, IList<eWebResource> webResources, string rootPath)
        {
            var filePath = descendant?.Attribute(WebResource.FilePath)?.Value;
            var wrName = descendant?.Attribute(WebResource.Name)?.Value;
            var description = descendant?.Attribute(WebResource.Description)?.Value;
            var type = descendant?.Attribute(WebResource.Type)?.Value;

            var webResourceFullName = packageName + wrName;
            var localFullName = rootPath + filePath.Replace("\\", "/");

            var webresourceInfo =
                new XElement(WebResource.WebResourceInfo,
                    new XAttribute(WebResource.Name, webResourceFullName),
                    new XAttribute(WebResource.FilePath, localFullName),
                    new XAttribute(WebResource.DisplayName, wrName),
                    new XAttribute(WebResource.Type, type),
                    new XAttribute(WebResource.Description, description));

            Console.WriteLine();
            Console.WriteLine(i + ") [" + DateTime.Now + "] " + webResourceFullName);

            if (IsWebResourceNameValid(webResourceFullName))
            {
                var resourceThatMayExist = webResources?.Where(w => w.Name.EndsWith(webResourceFullName)).FirstOrDefault();
                Guid? id;
                if (resourceThatMayExist != null)
                {
                    Console.WriteLine("Already exists.");
                    Console.WriteLine("Updating...");
                    crud.Update(webresourceInfo, resourceThatMayExist);
                    id = resourceThatMayExist.Id;
                }
                else
                {
                    Console.WriteLine("Doesn't exist. ");
                    Console.WriteLine("Creating...");
                    id = crud.Create(webresourceInfo);
                }

                return id;
            }

            log.Info(ValidNameMsg);
            return null;
        }

        public IEnumerable<eWebResource> RetrieveWebResourcesForActiveSolution()
        {
            var query = new QueryExpression
            {
                EntityName = eWebResource.EntityLogicalName,
                ColumnSet =
                    new ColumnSet(
                        eWebResource.PropertyNames.Content,
                        eWebResource.PropertyNames.Name,
                        eWebResource.PropertyNames.WebResourceType,
                        eWebResource.PropertyNames.DisplayName,
                        eWebResource.PropertyNames.Description,
                        eWebResource.PropertyNames.WebResourceId
                    ),
                NoLock = true,
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression(eWebResource.PropertyNames.IsManaged,
                            ConditionOperator.Equal,
                            false),
                        new ConditionExpression(eWebResource.PropertyNames.IsCustomizable,
                            ConditionOperator.Equal,
                            true)
                    }
                }
            };

            var link = query.AddLink(SolutionComponent.EntityLogicalName, eWebResource.PropertyNames.WebResourceId, SolutionComponent.PropertyNames.ObjectId);
            link.LinkCriteria.AddCondition(
                SolutionComponent.PropertyNames.ComponentType,
                ConditionOperator.Equal,
                (int) SolutionComponent.OptionSet.ComponentType.WebResource);
            link.LinkCriteria.AddCondition(
                SolutionComponent.PropertyNames.SolutionId,
                ConditionOperator.Equal,
                crud.ActiveSolution.SolutionId.Value);


            var fetchDefinitions = Service.RetrieveMultiple(query);

            return fetchDefinitions.Entities.Select(webR => webR.ToEntity<eWebResource>());
        }

        private bool IsWebResourceNameValid(string name)
        {
            var inValidWrNameRegex = new Regex("[^a-z0-9A-Z_\\./]|[/]{2,}", (RegexOptions.Compiled | RegexOptions.CultureInvariant));
            bool result = true;
            //Test valid characters
            if (inValidWrNameRegex.IsMatch(name))
            {
                log.Info(ValidNameMsg);
                result = false;
            }

            //Test length
            //Remove the customization prefix and leading _ 
            if (name.Remove(0, crud.ActivePublisher.CustomizationPrefix.Length + 1).Length > 100)
            {
                err.Error("Error: Web Resource name must be <= 100 characters.");
                result = false;
            }

            return result;
        }

    }

    public static class WebResource
    {
        public static readonly string WebResourceInfo = "WebResourceInfo";
        public static readonly string Name = "name";
        public static readonly string FilePath = "filePath";
        public static readonly string DisplayName = "displayName";
        public static readonly string Type = "type";
        public static readonly string Description = "description";
    }

    public static class PackageInfo
    {
        public static readonly string UtilityRoot = "UtilityRoot";
        public static readonly string Package = "Package";
        public static readonly string Packages = "Packages";
        public static readonly string Name = "name";
        public static readonly string RootPath = "rootPath";
    }
}
