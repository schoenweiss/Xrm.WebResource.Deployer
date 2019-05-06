# Xrm.WebResource.Deployer
This tool simplifies the deployment for single, multiple or all available web ressources to Microsoft Dynamics 365 / Dynamics CRM. It uses a packages.xml file where all available web resources and additional information are listed. It also generates a complete new packages.xml file from a folder structure holding all web resources.

## Options

    Usage - Xrm.WebResource.Deployer <action> -options

    GlobalOption   Description
    Help (-?)      Shows help

    Actions

    Deploy -options - Deploy web resource(s)

      Option                   Description
      ConnectionString* (-C)   Connection string to CRM system
      PackagesXml* (-P)        Specify path to package.xml
      FileName (-F)            File path of webresource. Related to root path, e.g: '\Forms\Account.js'
      FileNames (-Fi)          File paths of webresources. Related to root path, e.g: '\Forms\Account.js'
      PackageName (-Pa)        Package name, which will be updated and published. e.g. 'Entities\'
      PublishAll (-Pu)         If set to True: Updates, creates and publisches all available webresources [Default='False']
      Generate (-G)            Generates a new package.xml file from given path to web resource folder [Default='False']
      SourceFolder (-S)        Source folder for package.xml generation
      
