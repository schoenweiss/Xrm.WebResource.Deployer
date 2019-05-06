using System;

namespace Xrm.WebResource.Deployer.Utility
{
    public static class ResourceExtensions
    {
        //Provides the integer value for the type of Web Resource
        public enum WebResourceType
        {
            Html = 1,
            Css = 2,
            JScript = 3,
            Xml = 4,
            Png = 5,
            Jpg = 6,
            Gif = 7,
            Silverlight = 8,
            Stylesheet_XSL = 9,
            Ico = 10,
            Svg = 11,
            Resx = 12
        }

        public static WebResourceType ConvertStringExtension(string extensionValue)
        {
            if (extensionValue.StartsWith("."))
            {
                extensionValue = extensionValue.Remove(0, 1);
            }
            switch (extensionValue.ToLower())
            {
                case "css":
                    return WebResourceType.Css;
                case "xml":
                    return WebResourceType.Xml;
                case "gif":
                    return WebResourceType.Gif;
                case "htm":
                    return WebResourceType.Html;
                case "html":
                    return WebResourceType.Html;
                case "ico":
                    return WebResourceType.Ico;
                case "jpg":
                    return WebResourceType.Jpg;
                case "png":
                    return WebResourceType.Png;
                case "js":
                    return WebResourceType.JScript;
                case "xap":
                    return WebResourceType.Silverlight;
                case "xsl":
                    return WebResourceType.Stylesheet_XSL;
                case "xslt":
                    return WebResourceType.Stylesheet_XSL;
                case "svg":
                    return WebResourceType.Svg;
                case "resx":
                    return WebResourceType.Resx;
                default:
                    throw new ArgumentOutOfRangeException($"\"{extensionValue.ToLower()}\" is not recognized as a valid file extension for a Web Resource.");

            }
        }
    }
}
