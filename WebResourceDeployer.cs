using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using log4net;
using Microsoft.Xrm.Sdk;
using Xrm.WebResource.Deployer.Publisher;
using Xrm.WebResource.Deployer.Utility;

namespace Xrm.WebResource.Deployer
{
    /// <summary>
    /// WebRessource manager and its deploy method
    /// </summary>
    public class WebResourceDeployer
    {
        private CmdArgs Args { get; }
        private ObservableCollection< XElement > PackageCollection { get; set; }
        private IOrganizationService Service { get; set; }
        private readonly ILog err;
        private readonly ILog log;

        public WebResourceDeployer( CmdArgs args, ILog log, ILog err )
        {
            var packagesFilename = string.IsNullOrEmpty( args.PackagesXml ) ? throw new Exception( "Package.xml path was not given, please provide path." ) : args.PackagesXml;
            Args = args;

            this.log = log;
            this.err = err;

            Service = OrganizationServiceFactory.ConnectByConnectionString( args.ConnectionString );
            PackageCollection = FileLoader.LoadPackages( packagesFilename );
        }

        /// <summary>
        /// Handles all web ressource requests
        /// </summary>
        public void DeployWebresource( )
        {
            var crud = new Crud( Service, log, err );
            var manager = new WebResourceManager( Service, PackageCollection, crud, log, err, Args );
            var webResources = manager.RetrieveWebResourcesForActiveSolution( );
            manager.Manage( webResources );
        }
    }
}
