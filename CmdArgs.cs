using PowerArgs;

namespace Xrm.WebResource.Deployer
{
    /// <summary>
    /// Command line arguments
    /// </summary>
    [TabCompletion]
    public class CmdArgs
    {
        [ArgRequired, ArgDescription("Connection string to the CRM system")]
        public string ConnectionString { get; set; }

        [ArgRequired, ArgDescription("Specify path to package.xml")]
        public string PackagesXml { get; set; }

        [ArgDescription("File path of webresource. Related to root path, e.g: '\\Forms\\Account.js'")]
        public string FileName { get; set; }

        [ArgDescription("File paths of webresources. Related to root path, e.g: '\\Forms\\Account.js,\\Entities\\Account.js'")]
        public string[] FileNames { get; set; }

        [ArgDescription("Package name, which will be updated and published. e.g. 'Entities\\'")]
        public bool PackageName { get; set; }

        [ArgDescription("If set to True: Updates, creates and publisches all available webresources"), DefaultValue(false)]
        public bool PublishAll { get; set; }
    }

    public class GenerationArgs
    {
        [ArgDescription("Absolute path to source folder for package.xml generation"), DefaultValue("")]
        public string SourceFolder { get; set; }

        [ArgDescription("Prefix for web resource deployment on folder structure"), DefaultValue("")]
        public string DeployPrefix { get; set; }
    }
}
