using System;
using System.Collections.Generic;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace Xrm.WebResource.Deployer.Publisher
{
    /// <summary>
    /// Publish and deployment handling
    /// </summary>
    public class Publisher 
    {
        private readonly ILog log;
        private readonly ILog err;
        private IOrganizationService Service { get; }

        public Publisher(IOrganizationService service, ILog log, ILog err)
        {
            this.log = log;
            this.err = err;
            Service = service;
        }


        public void Publish(Guid webresourceId)
        {
            var request = new OrganizationRequest
            {
                RequestName = "PublishXml",
                Parameters = new ParameterCollection
                {
                    new KeyValuePair< string, object >( "ParameterXml",
                        $"<importexportxml><webresources>{string.Join( "", $"<webresource>{webresourceId}</webresource>" )}</webresources></importexportxml>" )
                }
            };


            Service.Execute(request);
        }

        private void PublishAll()
        {
            try
            {
                log.Info(String.Empty);
                log.Info("Publishing all customizations...");

                PublishAllXmlRequest publishRequest = new PublishAllXmlRequest();
                var response = (PublishAllXmlResponse)Service.Execute(publishRequest);

                log.Info("Done.");
            }
            catch (Exception e)
            {
                log.Info("Error publishing: " + e.Message);
            }
        }
    }
}
