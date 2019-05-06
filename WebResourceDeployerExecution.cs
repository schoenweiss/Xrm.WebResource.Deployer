using System;
using log4net;
using log4net.Config;
using PowerArgs;

namespace Xrm.WebResource.Deployer
{
    /// <summary>
    /// WebRessource Deployer
    /// </summary>
    [ ArgExceptionBehavior( ArgExceptionPolicy.StandardExceptionHandling ) ]
    public class WebResourceDeployerExecution
    {
        private readonly ILog err = LogManager.GetLogger( "root" );
        private readonly ILog log = LogManager.GetLogger( typeof( Program ) );

        /// <summary>
        /// Runner
        /// </summary>
        public WebResourceDeployerExecution( )
        {
            XmlConfigurator.Configure( );

        }

        /// <summary>
        /// Helper
        /// </summary>
        [ HelpHook, ArgShortcut( "-?" ), ArgDescription( "Shows help" ) ]
        // ReSharper disable once UnusedMember.Global
        public bool Help { get; set; }


        /// <summary>
        /// Deployer
        /// </summary>
        /// <param name="args"></param>
        [ ArgActionMethod, ArgDescription( "Deploy web resource(s)" ) ]
        // ReSharper disable once UnusedMember.Global
        public void Deploy( CmdArgs args )
        {
            StartDeployment( args, i => i.DeployWebresource( ) );
        }


        private void StartDeployment( CmdArgs args, Action< WebResourceDeployer > action )
        {
            try
            {
                if( string.IsNullOrEmpty( args.ConnectionString ) || string.IsNullOrEmpty( args.FileName ) )
                {
                    return;
                }

                var webResourceDeployer = new WebResourceDeployer( args, err, log );
                action( webResourceDeployer );
            }
            catch( Exception ex )
            {
                err.Error( $"Exception occured, terminating. Message: {ex.Message}", ex );
            }
        }
    }
}
