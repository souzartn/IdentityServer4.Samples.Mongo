# IdentityServer4 with  MongoDB configuration 

These samples are based on [IdentityServer4.quickstart.samples](https://github.com/IdentityServer/IdentityServer4.Samples) and aims to shows how to use MongoDB for the configuration data.

## Solution overview: 

###This repo contains 02 samples as per below:

- IdentityServer4-mongo: simple IdentityServer4 sample similar to [IdentityServer4.quickstart.samples](https://github.com/IdentityServer/IdentityServer4.Samples)
- IdentityServer4-mongo-AspIdentity: Sample based on IdentityServer4  and  Asp Identity 


###Important technical dependencies
- Solution is based on Visual Studio 2017.
- Nugets:
-- MongoDB.Driver
-- Microsoft.AspNetCore.Identity.MongoDB (for IdentityServer4-mongo-AspIdentity)


###Solution structure
- QuickstartIdentityServer -  project based on IdentityServer4 that will manage all authentication
- API - is a sample API project.
- clients/MvcClient - ASP .NET Core project sample
- clients/Client - .NET Core console project sample



## Implementation Walkthrought

TODO - add text here


 
### Next steps


- [Setup mongo](./mongodb.md)

- [Running solution](./running.md)

- [FAQ](./faq.md)


