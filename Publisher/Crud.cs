using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xrm.WebResource.Deployer.Entities;
using Xrm.WebResource.Deployer.Utility;

namespace Xrm.WebResource.Deployer.Publisher
{
    public class Crud
    {
        private readonly ILog err;
        private readonly ILog log;
        private IOrganizationService Service { get; }
        public Solution ActiveSolution { get; set; }
        public Entities.Publisher ActivePublisher { get; set; }
        private Solution SelectedSolution { get; set; }
        private ObservableCollection<Solution> UnmanagedSolutions { get; set; }


        /// <summary>
        /// Initialize CRUD manager for webressource deployment
        /// </summary>
        /// <param name="service"></param>
        /// <param name="log"></param>
        /// <param name="err"></param>
        public Crud(IOrganizationService service, ILog log, ILog err)
        {
            this.err = err;
            Service = service;
            this.log = log;

            //Check whether it already exists
            var queryUnmanagedSolutions = new QueryExpression
            {
                EntityName = Solution.EntityLogicalName,
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            queryUnmanagedSolutions.Criteria.AddCondition(Solution.PropertyNames.IsManaged, ConditionOperator.Equal, false);
            var querySolutionResults = service.RetrieveMultiple(queryUnmanagedSolutions);

            if (querySolutionResults.Entities.Count > 0)
            {
                //The Where() is important because a query for all solutions
                //where Type=Unmanaged returns 3 solutions. The CRM UI of a 
                //vanilla instance shows only 1 unmanaged solution: "Default". 
                //Assume "Active" and "Basic" should not be touched?
                UnmanagedSolutions = new ObservableCollection<Solution>(querySolutionResults.Entities.Select(x => x.ToEntity<Solution>()).Where(s => s?.UniqueName == "Default"));

                //If only 1 solution returns just go ahead and default it.
                if (UnmanagedSolutions.Count == 1 && UnmanagedSolutions[0].UniqueName == "Default")
                {
                    SelectedSolution = UnmanagedSolutions[0];
                    ActiveSolution = SelectedSolution;

                    SetActivePublisher(new OrganizationServiceContext(service));
                }
            }
        }

        private void SetActivePublisher(OrganizationServiceContext context)
        {
            if (ActiveSolution == null)
            {
                return;
            }

            var fetchDefinitions = Service.RetrieveMultiple(new QueryExpression
            {
                EntityName = Entities.Publisher.EntityLogicalName,
                ColumnSet = new ColumnSet(Entities.Publisher.PropertyNames.CustomizationPrefix),
                NoLock = true,
                Criteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression(Entities.Publisher.PropertyNames.PublisherId,
                            ConditionOperator.Equal,
                            ActiveSolution.PublisherId.Id)
                    }
                }
            });

            ActivePublisher = fetchDefinitions.Entities.First().ToEntity<Entities.Publisher>();
        }

        /// <summary>
        /// Create a new webressource
        /// </summary>
        /// <param name="webResourceInfo"></param>
        public Guid? Create(XElement webResourceInfo)
        {
            try
            {
                var type =
                    (int) ResourceExtensions.ConvertStringExtension(webResourceInfo?.Attribute(WebResource.Type)?.Value);
                var optionsetValue = new OptionSetValue(type);
                var fullName = GetWebResourceFullNameIncludingPrefix(webResourceInfo);
                //Create the Web Resource.
                var wr = new Entities.WebResource
                {
                    Content = GetEncodedFileContents(webResourceInfo?.Attribute(WebResource.FilePath)?.Value),
                    DisplayName = webResourceInfo?.Attribute(WebResource.DisplayName)?.Value,
                    Description = webResourceInfo?.Attribute(WebResource.Description)?.Value,
                    LogicalName = Entities.WebResource.EntityLogicalName,
                    Name = fullName,
                    WebResourceType = optionsetValue
                };

                var guid = Service.Create(wr);

                //If not the "Default Solution", create a SolutionComponent to assure it gets
                //associated with the ActiveSolution. Web Resources are automatically added
                //as SolutionComponents to the Default Solution.
                if (ActiveSolution.UniqueName != "Default")
                {
                    var scRequest =
                        new AddSolutionComponentRequest
                        {
                            ComponentType = (int) SolutionComponent.OptionSet.ComponentType.WebResource,
                            SolutionUniqueName = ActiveSolution.UniqueName,
                            ComponentId = guid
                        };
                    Service.Execute(scRequest);
                }

                return guid;
            }
            catch (Exception e)
            {
                err.Error("Error: " + e.Message);
                return null;
            }
        }

        private string GetWebResourceFullNameIncludingPrefix(XElement webResourceInfo)
        {
            var name = ActivePublisher.CustomizationPrefix + "_/";
            name += webResourceInfo?.Attribute(WebResource.Name)?.Value;
            return name;
        }

        /// <summary>
        /// Updates a webressource
        /// </summary>
        /// <param name="webResourceInfo"></param>
        /// <param name="existingResource"></param>
        public void Update(XElement webResourceInfo, Entities.WebResource existingResource)
        {
            try
            {
                var fullName = GetWebResourceFullNameIncludingPrefix(webResourceInfo);
                var wr = new Entities.WebResource
                {
                    Content = GetEncodedFileContents(webResourceInfo?.Attribute(WebResource.FilePath)?.Value),
                    DisplayName = webResourceInfo?.Attribute(WebResource.DisplayName)?.Value,
                    Description = webResourceInfo?.Attribute(WebResource.Description)?.Value,
                    Name = fullName,
                    WebResourceId = existingResource.WebResourceId
                };
                Service.Update(wr);
            }
            catch (Exception e)
            {
                err.Error("Error: " + e.Message);
            }
        }

        private static string GetEncodedFileContents(string pathToFile)
        {
            FileStream fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            byte[] binaryData = new byte[fs.Length];
            fs.Read(binaryData, 0, (int) fs.Length);
            fs.Close();
            return Convert.ToBase64String(binaryData, 0, binaryData.Length);
        }

    }
}
