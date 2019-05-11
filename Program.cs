using System;
using PowerArgs;

namespace Xrm.WebResource.Deployer
{
    /// <summary>
    /// WebResource Deployer
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Runner
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Args.InvokeAction<WebResourceDeployerExecution>(args);
            Environment.Exit(0);
        }
    }
}
