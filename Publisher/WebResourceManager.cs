using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Instrumentation;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Xrm.WebResource.Deployer.Entities;
using eWebResource = Xrm.WebResource.Deployer.Entities.WebResource;

namespace Xrm.WebResource.Deployer.Publisher
{
    public class WebResourceManager
    {
        private const string ValidNameMsg = "ERROR: Web Resource names cannot contain spaces or hyphens. They must be alphanumeric and contain underscore characters, periods, and non-consecutive forward slash characters";

        private readonly ObservableCollection< XElement > packages;
        private readonly Crud crud;
        private IOrganizationService Service { get; }
        private CmdArgs Args { get; }
        private readonly bool processAll;
        private readonly ILog err;
        private readonly ILog log;

        public WebResourceManager( IOrganizationService service, ObservableCollection< XElement > packageCollection, Crud crud, ILog log, ILog err, CmdArgs args )
        {
            packages = packageCollection;
            this.crud = crud;
            Service = service;
            Args = args;
            processAll = args.PublishAll;
            this.err = err;
            this.log = log;
        }

        public void Manage( IEnumerable< Entities.WebResource > crmResources )
        {
            if( !Args.PublishAll && string.IsNullOrEmpty( Args.FileName ) && Args.FileNames.Length == 0 && !Args.PackageName )
            {
                err.Error( "No option was given to deploy web ressource to CRm. Please provide at least one." );
            }

            var webResources = crmResources as IList< Entities.WebResource > ?? crmResources.ToList( );



            int i = 1;
            bool found = false;
            foreach( var package in packages )
            {
                string packageName = package?.Attribute( PackageInfo.Name )?.Value;
                string rootPath = package?.Attribute( PackageInfo.RootPath )?.Value;

                foreach( var descendant in package?.Descendants( )?.ToList( ) )
                {
                    if( processAll )
                    {
                        ManageSingle( descendant, packageName, i, webResources, rootPath );
                        i++;
                    }
                    else
                    {
                        var dName = descendant?.Attribute( WebResource.FilePath )?.Value;
                        if( dName != Args.FileName )
                        {
                            continue;
                        }

                        ManageSingle( descendant, packageName, i, webResources, rootPath );
                        found = true;
                        i++;
                    }
                }

                if( !processAll && found ) break;
            }

            if( !processAll && !found ) err.Error( "The file path was not found in Package.xml !" );

        }

        private void ManageSingle( XElement descendant, string packageName, int i, IList< Entities.WebResource > webResources, string rootPath )
        {
            var filePath = descendant?.Attribute( WebResource.FilePath )?.Value;
            var displayName = descendant?.Attribute( WebResource.DisplayName )?.Value;
            var wrName = descendant?.Attribute( WebResource.Name )?.Value;
            var description = descendant?.Attribute( WebResource.Description )?.Value;
            var type = descendant?.Attribute( WebResource.Type )?.Value;

            var webResourceFullName = packageName + wrName;
            var localFullName = (rootPath + filePath).Replace( "\\", "/" );

            var webresourceInfo =
                new XElement( WebResource.WebResourceInfo,
                              new XAttribute( WebResource.Name, webResourceFullName ),
                              new XAttribute( WebResource.FilePath, localFullName ),
                              new XAttribute( WebResource.DisplayName, wrName ?? "" ),
                              new XAttribute( WebResource.Type, type ?? throw new InvalidOperationException( $"Type of web resource {webResourceFullName} not given in package.xml" ) ),
                              new XAttribute( WebResource.Description, description ?? "" ) );

            Console.WriteLine( );
            Console.WriteLine( i + ") [" + DateTime.Now + "] " + webResourceFullName );

            if( IsWebResourceNameValid( webResourceFullName ) )
            {
                var resourceThatMayExist = webResources?.Where( w => w.Name.EndsWith( webResourceFullName ) ).FirstOrDefault( );
                if( resourceThatMayExist != null )
                {
                    Console.WriteLine( "Already exists." );
                    Console.WriteLine( "Updating..." );
                    crud.Update( webresourceInfo, resourceThatMayExist );

                    Console.WriteLine( "Publishing..." );
                    var publisher = new Publisher( Service, log, err );
                    publisher.Publish( resourceThatMayExist.Id );
                }
                else
                {
                    Console.WriteLine( "Doesn't exist. " );
                    Console.WriteLine( "Creating..." );
                    crud.Create( webresourceInfo );
                }

                Console.WriteLine( "Done." );
            }
            else
            {
                log.Info( ValidNameMsg );
            }
        }

        public IEnumerable< eWebResource > RetrieveWebResourcesForActiveSolution( )
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
                (int)SolutionComponent.OptionSet.ComponentType.WebResource);
            link.LinkCriteria.AddCondition(
                SolutionComponent.PropertyNames.SolutionId,
                ConditionOperator.Equal,
                crud.ActiveSolution.SolutionId.Value);


            var fetchDefinitions = Service.RetrieveMultiple(query);
          
            return fetchDefinitions.Entities.Select(webR => webR.ToEntity<eWebResource>());
        }

        private bool IsWebResourceNameValid( string name )
        {
            var inValidWrNameRegex = new Regex( "[^a-z0-9A-Z_\\./]|[/]{2,}", ( RegexOptions.Compiled | RegexOptions.CultureInvariant ) );
            bool result = true;
            //Test valid characters
            if( inValidWrNameRegex.IsMatch( name ) )
            {
                log.Info( ValidNameMsg );
                result = false;
            }

            //Test length
            //Remove the customization prefix and leading _ 
            if( name.Remove( 0, crud.ActivePublisher.CustomizationPrefix.Length + 1 ).Length > 100 )
            {
                err.Error( "Error: Web Resource name must be <= 100 characters." );
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
