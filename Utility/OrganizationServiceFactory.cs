using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace Xrm.WebResource.Deployer.Utility
{
    /// <summary>
    /// Connect to CRM via different factory methods
    /// </summary>
    public static class OrganizationServiceFactory
    {
        /// <summary>
        /// Connect to CRM with given connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public static IOrganizationService ConnectByConnectionString( string connectionString )
        {
            var conn = new CrmServiceClient( connectionString );

            if( !conn.IsReady )
            {
                throw new Exception( $"Error during establishing of CRM connection: {conn.LastCrmError}" );
            }

            conn.OrganizationServiceProxy.Timeout = new TimeSpan( 0, 10, 0 ); // default 2 minutes
            return conn.OrganizationWebProxyClient != null
                ? ( IOrganizationService ) conn.OrganizationWebProxyClient
                : conn.OrganizationServiceProxy;
        }
    }
}
