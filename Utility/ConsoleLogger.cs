using System;
using log4net;
using log4net.Config;
using Microsoft.Xrm.Sdk;

namespace Xrm.WebResource.Deployer.Utility
{
    public class ConsoleLogger: ITracingService
    {
        private ILog err;
        private ILog log;

        public ConsoleLogger( Type type )
        {
            err = LogManager.GetLogger( "root" ); // error
            log = LogManager.GetLogger( type );

            XmlConfigurator.Configure( );
        }

        public void Trace( string format, params object[] args )
        {
            Log( format, args );
        }

        public void Log( string format, params object[] args )
        {
            log.InfoFormat( format, args );
        }

        public void Error( string format, params object[] args )
        {
            err.ErrorFormat( format, args );
        }
    }
}
