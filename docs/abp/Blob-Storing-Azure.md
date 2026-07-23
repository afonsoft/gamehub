# https://aspnetboilerplate.com/Pages/Documents/Blob-Storing-Azure

## Blob Storing Azure

|     |     |     |
| --- | --- | --- |
| |     |     |
| --- | --- |
|  | × | | search |  |

Custom Search

|     |     |
| --- | --- |
|  | Sort by<br>Relevance<br>Date |

Version

latest (v10.4)v10.3v10.2v10.1v10.0v9.4.2v9.4.1v9.4.0v9.3.0v9.2.0v9.1.3v9.1v9.0v8.4v8.3v8.2v8.1v8.0v7.4v7.3v7.2v7.1v7.0-rc1v7.0v6.6.1v6.6.0v6.5.0v6.4-rc1v6.4.0v6.3.1v6.3v6.2v6.1.1v6.1.0v6.0v5.14v5.13v5.12v5.10.1v5.10v5.9v5.8v5.7v5.6v5.5v5.4v5.3v5.2v5.1.0v5.0.0v4.21v4.20v4.19v4.18v4.17v4.16v4.15v4.14v4.13v4.12v4.11.0v4.10.1v4.10.0v4.9.0v4.8.1v4.8.0v4.7.0v4.6.0v4.5.0v4.4.0v4.3.0v4.2.0v4.1.0v4.0.2v4.0.1v4.0.0v3.9.0v3.8.3v3.8.2v3.8.1v3.8.0v3.7.2v3.7.1v3.7.0v3.6.2v3.6.1v3.6.0v3.5.0v3.4.0v3.3.0v3.2.5v3.2.4v3.2.3v3.2.2v3.2.1v3.2.0v3.1.2v3.1.1v3.1.0v3.0.0-beta3v3.0.0-rc2v3.0.0-beta2v3.0.0-rc1v3.0.0-beta1v3.0.0v2.3.0v2.2.2v2.2.1v2.2.0v2.1.3v2.1.2v2.1.1v2.1.0-beta4v2.1.0-beta3v2.1.0-beta2v2.1.0-beta1v2.1.0v2.0.2v2.0.1v2.0.0-preview4v2.0.0-rc3v2.0.0-preview3v2.0.0-rc2v2.0.0-preview1v2.0.0v2.0.0-rcv1.5.2v1.5.1v1.5.0v1.4.3v1.4.2v1.4.1v1.4.0.0v1.3.1.0v1.3.0.0v1.2.2.0v1.2.1.0v1.2.0.0v1.1.3.0v1.1.1.0v1.1.0.0v1.0.0.0v0.10.3.2Menu

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Blob-Storing-Azure.md)

In this document

# BLOB Storing Azure Provider [Anchor](https://aspnetboilerplate.com/Pages/Documents/Blob-Storing-Azure\#blob-storing-azure-provider)

BLOB Storing Azure Provider can store BLOBs in [Azure Blob storage](https://azure.microsoft.com/en-us/services/storage/blobs/).

## Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Blob-Storing-Azure\#installation)

Install the `Abp.BlobStoring.Azure` NuGet package to your project and add `[DependsOn(typeof(AbpBlobStoringAzureModule))]` to the ABP module class inside your project.

## Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Blob-Storing-Azure\#configuration)

Configuration is done in the Initialize method of your module class.

Example: Configure to use the azure storage provider by default

Copy

```csharp
Configuration.Modules.AbpBlobStoring().Containers.Configure<AbpBlobStoringOptions>(options =>
{
    options.Containers.ConfigureDefault(container =>
    {
        container.UseAzure(azure =>
        {
            azure.ConnectionString = "your azure connection string";
            azure.ContainerName = "your azure container name";
            azure.CreateContainerIfNotExists = true;
        });
    });
});
```

### Options [Anchor](https://aspnetboilerplate.com/Pages/Documents/Blob-Storing-Azure\#options)

- **ConnectionString (string)**: A connection string includes the authorization information required for your application to access data in an Azure Storage account at runtime using Shared Key authorization. Please refer to Azure documentation: https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string
- **ContainerName (string)**: You can specify the container name in azure. If this is not specified, it uses the name of the BLOB container defined with the BlobContainerName attribute (see the BLOB storing document). Please note that Azure has some rules for naming containers. A container name must be a valid DNS name, conforming to the following naming rules:

  - Container names must start or end with a letter or number, and can contain only letters, numbers, and the dash (-) character.
  - Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
  - All letters in a container name must be lowercase.
  - Container names must be from 3 through 63 characters long.
- **CreateContainerIfNotExists (bool)**: Default value is false, If a container does not exist in azure, AzureBlobProvider will try to create it.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe