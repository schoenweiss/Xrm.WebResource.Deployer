# Xrm.WebResource.Deployer
This tool simplifies the deployment for single, multiple or all available supported web ressources to Microsoft Dynamics 365 / Dynamics CRM. It uses a packages.xml file where all available web resources and additional information are listed. It also generates a complete new packages.xml file from a folder structure holding all web resources. This packages.xml can also be used in the [WebResource-Utility-Tool](https://code.msdn.microsoft.com/Web-Resource-Utility-sample-eb3771e9)

## Options

    Usage - Xrm.WebResource.Deployer <action> -options
    GlobalOption   Description
    Help (-?)      Shows help

    Actions
    
    Deploy -options - Deploy web resource(s)

        Option                   Description
        ConnectionString* (-C)   Connection string to the CRM system
        PackagesXml* (-P)        Specify path to package.xml
        FileName (-F)            File path of webresource. Related to root path, e.g: '\Forms\Account.js'
        FileNames (-Fi)          File paths of webresources. Related to root path, e.g: '\Forms\Account.js,\Entities\Account.
                                 js'
        PackageName (-Pa)        Package name, which will be updated and published. e.g. 'Entities\'
        PublishAll (-Pu)         If set to True: Updates, creates and publisches all available webresources
                                 [Default='False']
                                 
    Generate -options - Generates a new package.xml file from given source folder

        Option              Description
        SourceFolder (-S)   Absolute path to source folder for package.xml generation [Default='']
        DeployPrefix (-D)   Prefix for web resource deployment on folder structure [Default='']

## Scenarios
1. Deploy a single web resource to the called CRM system
```Bash
Xrm.WebResource.Deployer.exe Deploy -C "connectionString" -P "pathTo\packages.xml" -F "relativeFilePathTo\WebResource.js"
```
2. Deploy multiple web resources to the called CRM system
```Bash
Xrm.WebResource.Deployer.exe Deploy -C "connectionString" -P "pathTo\packages.xml" -Fi "relativeFilePathTo\WebResource.js,"relativeFilePathTo\WebResource.html"
```
3. Deploy all listed web resources to the called CRM system
```Bash
Xrm.WebResource.Deployer.exe Deploy -C "connectionString" -P "pathTo\packages.xml" -Pu true
```
4. Generate a new package.xml file from given source folder (not implemented yet)
```Bash
Xrm.WebResource.Deployer.exe Generate -S "pathTo\project" -D "Web"
```
