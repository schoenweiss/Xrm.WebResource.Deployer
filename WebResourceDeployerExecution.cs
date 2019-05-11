using System;
using log4net;
using log4net.Config;
using PowerArgs;

namespace Xrm.WebResource.Deployer
{
    /// <summary>
    /// WebRessource Deployer
    /// </summary>
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class WebResourceDeployerExecution
    {
        private readonly ILog err = LogManager.GetLogger("root");
        private readonly ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Runner
        /// </summary>
        public WebResourceDeployerExecution()
        {
            XmlConfigurator.Configure();

        }

        /// <summary>
        /// Helper
        /// </summary>
        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows help")]
        // ReSharper disable once UnusedMember.Global
        public bool Help { get; set; }


        /// <summary>
        /// Deployer
        /// </summary>
        /// <param name="args"></param>
        [ArgActionMethod, ArgDescription("Deploy web resource(s)")]
        // ReSharper disable once UnusedMember.Global
        public void Deploy(CmdArgs args)
        {
            StartDeployment(args, i => i.DeployWebresource());
        }

        /// <summary>
        /// Deployer
        /// </summary>
        /// <param name="args"></param>
        [ArgActionMethod, ArgDescription("Generates a new package.xml file from given source folder")]
        // ReSharper disable once UnusedMember.Global
        public void Generate(GenerationArgs args)
        {
            StartGeneration(args, i => i.GeneratePackagesXml());
        }


        private void StartDeployment(CmdArgs args, Action<WebResourceDeployer> action)
        {
            try
            {
                if (string.IsNullOrEmpty(args.ConnectionString) || string.IsNullOrEmpty(args.FileName) && !args.PublishAll && args.FileNames == null)
                {
                    Console.WriteLine("Please maintain correct execution parameter to deploy webresources to the designated CRM system.");
                    Environment.Exit(1);
                }

                var webResourceDeployer = new WebResourceDeployer(args, log, err);
                action(webResourceDeployer);
            }
            catch (Exception ex)
            {
                err.Error($"Exception occured, terminating. Message: {ex.Message}", ex);
            }
        }


        private void StartGeneration(GenerationArgs args, Action<PackageGenerator> action)
        {
            try
            {
                if (args.SourceFolder == "")
                {
                    Console.WriteLine("Please maintain correct source folder which holds all web resources of project.");
                    Environment.Exit(1);
                }

                var generator = new PackageGenerator(args, log, err);
                action(generator);
            }
            catch (Exception ex)
            {
                log.Error($"Exception occured, terminating. Message: {ex.Message}", ex);
                Environment.Exit(1);
            }
        }
    }
}
