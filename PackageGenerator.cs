using System;
using log4net;

namespace Xrm.WebResource.Deployer
{
    public class PackageGenerator
    {

        private GenerationArgs Args { get; }
        private readonly ILog err;
        private readonly ILog log;

        public PackageGenerator(GenerationArgs args, ILog log, ILog err)
        {
            Args = args;
            this.log = log;
            this.err = err;
        }

        public void GeneratePackagesXml()
        {
            Console.WriteLine("Function not implemented yet.");
            Environment.Exit(1);
        }
    }
}
