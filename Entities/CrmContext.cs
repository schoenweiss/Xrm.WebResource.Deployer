namespace Xrm.WebResource.Deployer.Entities
{
    /// <summary>
    /// Represents a source of entities bound to a CRM service. It tracks and manages changes made to the retrieved entities.
    /// </summary>
    public partial class CrmContext: Microsoft.Xrm.Sdk.Client.OrganizationServiceContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public CrmContext( Microsoft.Xrm.Sdk.IOrganizationService service ):
            base( service ) { }
    }
}
        