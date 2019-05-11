using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Xrm.WebResource.Deployer.Publisher
{
    /// <summary>
    /// Publish and deployment handling
    /// </summary>
    public class Publisher
    {
        private IOrganizationService Service { get; }

        public Publisher(IOrganizationService service)
        {
            Service = service;
        }

        public void Publish(List<Guid> webresourceIds)
        {
            var ids = string.Join("", webresourceIds.Select(id => $"<webresource>{id}</webresource>").ToList());

            var request = new OrganizationRequest
            {
                RequestName = "PublishXml",
                Parameters = new ParameterCollection
                {
                    new KeyValuePair<string, object>("ParameterXml",
                        $"<importexportxml><webresources>{ids}</webresources></importexportxml>")
                }
            };


            Service.Execute(request);
        }
    }
}
