## Mongo Implementation overview

Both  _QuickstartIdentityServer_ projects samples follow similar approach to use MongoDB for configuration data.

1. IdentityServer4 configuration to use mongo as repository is based on DI and _IdentityServerBuilderExtensions_ - Similar to  
[8_EntityFrameworkStorage sample](https://github.com/IdentityServer/IdentityServer4.Samples/tree/release/Quickstarts/8_EntityFrameworkStorage) implementation - Hence 
we perform all "initial plumbing" in ConfigureServices() method at startup.cs.

>		        public void ConfigureServices(IServiceCollection services)
>		{ 
>		...
>         // ---  configure identity server with MONGO Repository for stores, keys, clients and scopes ---
>         services.AddIdentityServer()
>                .AddTemporarySigningCredential()
>                .AddMongoRepository()
>                .AddClients()
>                .AddIdentityApiResources()
>                .AddPersistedGrants()
>                .AddTestUsers(Config.GetUsers());
>		...
>		}

2. In the  _IdentityServerBuilderExtensions.cs_ extension hold methods to enable  DI related work executed by ConfigureServices() as listed above. It's important to highlight that AddMongoRepository() method configure a simple _Repository Design Pattern_ to handle persistence to MongoDB. _IdentityServerBuilderExtensions.cs_
configure DI to use custom implementation for required interfaces such as  IClientStore, IResourceStore and IPersistedGrantStore enabling IdentityServer4 to interact with MongoDB seamlessly.

3. After that we just need to make sure all required IdentityServer4 Interfaces, listed above, are implemented leveraging MongoRepository.
e.g. :

>  public class CustomResourceStore : IResourceStore
>   {
>		...
>		private IEnumerable<ApiResource> GetAllApiResources()
>       {
>            return _dbRepository.All<ApiResource>();
>       }
>      ...
>	}
