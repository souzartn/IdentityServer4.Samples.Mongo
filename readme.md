# IdentityServer4 with  MongoDB configuration 

These samples are based on [IdentityServer4.quickstart.samples](https://github.com/IdentityServer/IdentityServer4.Samples) and aims to shows how to use MongoDB for the configuration data.

## Solution overview: 

### This repo contains 02 samples as per below:

* IdentityServer4-mongo: simple IdentityServer4 sample similar to [IdentityServer4.quickstart.samples](https://github.com/IdentityServer/IdentityServer4.Samples)
* IdentityServer4-mongo-AspIdentity: Sample based on IdentityServer4  and  Asp Identity 


### Important technical dependencies

* Solution is based on Visual Studio 2017.
* ASP .Net Core
* Nugets:
	* MongoDB.Driver
	* Microsoft.AspNetCore.Identity.MongoDB (for IdentityServer4-mongo-AspIdentity sample)


### Solution structure

* QuickstartIdentityServer -  project based on IdentityServer4 that manages authentication
* API - is a sample API project, used by  client/* projects to showcase QuickstartIdentityServer functionality
* clients/MvcClient - ASP .NET Core client project sample
* clients/Client - .NET Core console client project sample

- - - -

## Implementation Walkthrought

1. IdentityServer4 configuration to use mongo as repository is based on DI and IdentityServerBuilderExtensions - Similar to  
[8_EntityFrameworkStorage sample](https://github.com/IdentityServer/IdentityServer4.Samples/tree/release/Quickstarts/8_EntityFrameworkStorage) implementation. Hence 
we perform all "initial plumbing" on ConfigureServices() method at startup.cs.

'
            // ---  configure identity server with MONGO Repository for stores, keys, clients and scopes ---
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddMongoRepository()
                .AddClients()
                .AddIdentityApiResources()
                .AddPersistedGrants()
                .AddTestUsers(Config.GetUsers());
'

2. The IdentityServerBuilderExtensions extension provides methods to enable  DI related work executed by ConfigureServices() as listed above. 
It's important to highlight that AddMongoRepository() method configure a Simple Repository (Design Pattern) to handle persistence to MongoDB. Other that, this class 
configure other custom pieces   such as IClientStore, IResourceStore and IPersistedGrantStore enabling IdentityServer4 interact with MongoDB seamlessly.

3. Based on that, we just need to make sure all required IdentityServer4 Interfaces, listed above, are implemented leveraging MongoRepository.
e.g. Please refer to a piece of "CustomResourceStore" below:
'
...
		private IEnumerable<ApiResource> GetAllApiResources()
        {
            return _dbRepository.All<ApiResource>();
        }
...
'
 
### Next steps

*  [Setup mongo](./mongodb.md)

*  [Running solution](./running.md)

*  [FAQ](./faq.md)


