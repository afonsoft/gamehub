# https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing

## BLOB Storing

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/BLOB-Storing.md)

In this document

# BLOB Storing [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#blob-storing)

Storing binary objects a.k.a. [BLOB](https://en.wikipedia.org/wiki/Binary_large_object) s is a feature most modern apps need. ASP.NET Boilerplate provides an infrastructure for storing BLOBs.
BLOBs can be stored in a file system, database or on a cloud provider.

ASP.NET Boilerplate provides an abstraction to work with BLOBs and provides some pre-built storage providers that you can easily integrate to. Having such an abstraction has some benefits;

- You can easily integrate to your favorite BLOB storage provides with a few lines of configuration.
- You can then easily change your BLOB storage without changing your application code.
- If you want to create reusable application modules, you don't need to make assumption about how the BLOBs are stored.

## BLOB Storage Providers [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#blob-storage-providers)

ASP.NET Boilerplate offers BLOB providers below out of the box;

- [File System](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing-File-System): Stores BLOBs in a folder of the local file system, as standard files.
- [Azure](https://aspnetboilerplate.com/Pages/Documents/Blob-Storing-Azure): Stores BLOBs on the [Azure BLOB storage](https://azure.microsoft.com/en-us/services/storage/blobs/).

## Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#installation)

[Abp.BlobStoring](https://www.nuget.org/packages/Abp.BlobStoring) is the main package that defines the BLOB storing services. You can use this package to use the BLOB Storing system without depending a specific storage provider.

Install the `Abp.BlobStoring` NuGet package to your project and add \[DependsOn(typeof(AbpBlobStoringModule))\] to the ABP module class inside your project.

## The IBlobContainer [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#the-iblobcontainer)

`IBlobContainer` is the main interface to read and write BLOBs. An application may have multiple containers and each container can be separately configured. But, there is a default container that can be simply used by injecting the IBlobContainer.

Example: Simply read and write bytes of a named BLOB;

Copy

```csharp
namespace DemoApp
{
    public class MyService : ITransientDependency
    {
        private readonly IBlobContainer _blobContainer;

        public MyService(IBlobContainer blobContainer)
        {
            _blobContainer = blobContainer;
        }

        public async Task SaveBytesAsync(byte[] bytes)
        {
            await _blobContainer.SaveAsync("sample-blob", bytes);
        }

        public async Task<byte[]> GetBytesAsync()
        {
            return await _blobContainer.GetAllBytesOrNullAsync("sample-blob");
        }
    }
}
```

This service saves the given bytes with the `sample-blob` name and then gets the previously saved bytes with the same name.
`IBlobContainer` can work with `Stream` and `byte[]` objects, which will be detailed in the next sections.

### Saving BLOBs [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#saving-blobs)

`SaveAsync` method is used to save a new BLOB or replace an existing BLOB. `SaveAsync` gets the following parameters:

- **name (string):** Unique name of the BLOB.
- **stream (Stream) or bytes (byte\[\]):** The stream to read the BLOB content or a byte array.
- **overrideExisting (bool):** Set true to replace the BLOB content if it does already exists. Default value is false and throws BlobAlreadyExistsException if there is already a BLOB in the container with the same name.

### Reading/Getting BLOBs [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#reading-getting-blobs)

- `GetAsync`: Only gets a BLOB name and returns a Stream object that can be used to read the BLOB content. Always dispose the stream after using it. This method throws exception, if it can not find the BLOB with the given name.
- `GetOrNullAsync`: In opposite to the GetAsync method, this one returns null if there is no BLOB found with the given name.
- `GetAllBytesAsync`: Returns a byte\[\] instead of a Stream. Still throws exception if can not find the BLOB with the given name.
- `GetAllBytesOrNullAsync`: In opposite to the GetAllBytesAsync method, this one returns null if there is no BLOB found with the given name.

### Deleting BLOBs [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#deleting-blobs)

`DeleteAsync` method gets a BLOB name and deletes the BLOB data. It doesn't throw any exception if given BLOB was not found. Instead, it returns a bool indicating that the BLOB was actually deleted or not, if you care about it.

### Other Methods [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#other-methods)

- `ExistsAsync` method simply checks if there is a BLOB in the container with the given name.

## Typed IBlobContainer [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#typed-iblobcontainer)

Typed BLOB container system is a way of creating and managing multiple containers in an application;

- **Each container is separately stored.** That means the BLOB names should be unique in a container and two BLOBs with the same name can live in different containers without effecting each other.
- **Each container can be separately configured**, so each container can use a different storage provider based on your configuration.

To create a typed container, you need to create a simple class decorated with the `BlobContainerName` attribute:

Copy

```csharp
using Volo.Abp.BlobStoring;

namespace DemoApp
{
    [BlobContainerName("profile-pictures")]
    public class ProfilePictureContainer
    {

    }
}
```

Once you create the container class, you can inject `IBlobContainer<T>` for your container type.

Example:

Copy

```csharp
public class ProfileAppService : ApplicationService
{
    private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;

    public ProfileAppService(IBlobContainer<ProfilePictureContainer> blobContainer)
    {
        _blobContainer = blobContainer;
    }

    public async Task SaveProfilePictureAsync(byte[] bytes)
    {
        var blobName = AbpSession.ToUserIdentifier().ToString();
        await _blobContainer.SaveAsync(blobName, bytes);
    }

    public async Task<byte[]> GetProfilePictureAsync()
    {
        var blobName = AbpSession.ToUserIdentifier().ToString();
        return await _blobContainer.GetAllBytesOrNullAsync(blobName);
    }
}
```

### The Default Container [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#the-default-container)

If you don't use the generic argument and directly inject the `IBlobContainer` (as explained before), you get the default container. Another way of injecting the default container is using `IBlobContainer<DefaultContainer>`, which returns exactly the same container.

The name of the default container is `default`.

### Named Containers [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#named-containers)

Typed containers are just shortcuts for named containers. You can inject and use the `IBlobContainerFactory` to get a BLOB container by its name:

Copy

```csharp
public class ProfileAppService : ApplicationService
{
    private readonly IBlobContainer _blobContainer;

    public ProfileAppService(IBlobContainerFactory blobContainerFactory)
    {
        _blobContainer = blobContainerFactory.Create("profile-pictures");
    }

    //...
}
```

## IBlobContainerFactory [Anchor](https://aspnetboilerplate.com/Pages/Documents/BLOB-Storing\#iblobcontainerfactory)

`IBlobContainerFactory` is the service that is used to create the BLOB containers. One example was shown above.

Example: Create a container by name

Copy

```csharp
var blobContainer = blobContainerFactory.Create("profile-pictures");
```

Example: Create a container by type

Copy

```csharp
var blobContainer = blobContainerFactory.Create<ProfilePictureContainer>();
```

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe