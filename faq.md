

## FAQ 

### What interfaces were implemented, how and where they are registered and used?

- Bulk of the "magic" takes place at [startup.cs](https://github.com/Rilton/IdentityServer4.Samples.Mongo/blob/dd2fc4f2e9d375c941db5c6042e1e2132ac8209c/9_MongoRepository/src/QuickstartIdentityServer/Startup.cs) and  
[IdentityServerBuilderExtensions.cs](https://github.com/Rilton/IdentityServer4.Samples.Mongo/blob/dd2fc4f2e9d375c941db5c6042e1e2132ac8209c/9_MongoRepository/src/QuickstartIdentityServer/Quickstart/Extension/IdentityServerBuilderExtensions.cs). A quick
look at those classes should shed some light.

- MongoDB connectivity is based on  nugets and  on a simple Repository Design Pattern implementation. See  interface/IRepository & /interface/MongoRepository classes - everything comes togheter with .NET Core Dependency Injection (see startup.cs & IdentityServerBuilderExtensions.cs)  

