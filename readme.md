# IdentityServer4 with  MongoDB configuration 

This project is based on [IdentityServer4.quickstart.samples] (https://github.com/IdentityServer/IdentityServer4.Samples) and aims to shows how to use MongoDB for the configuration data.

#### Additional information: 

- Solution is based on Visual Studio 2017.
- QuickstartIdentityServer - is the real IdentityServer4 project that will manage all authentication
- API - is a sample API project.
- clients/* - contains varios sample client projects.

### Is this source code really similar to IdentityServer4.quickstart.samples?
This sample was based on [Quickstart #8: EntityFramework configuration](https://github.com/IdentityServer/IdentityServer4.Samples/tree/release/Quickstarts/8_EntityFrameworkStorage), therefore  most of the code is identical to that sample, with monst of the change restrict to parts related to the use of MongoDB as database.


### What interfaces were implemented, how and where they are registered and used?

- Bulk of the "magic" takes place at [startup.cs](https://github.com/Rilton/IdentityServer4.Samples.Mongo/blob/dd2fc4f2e9d375c941db5c6042e1e2132ac8209c/9_MongoRepository/src/QuickstartIdentityServer/Startup.cs) and  
[IdentityServerBuilderExtensions.cs](https://github.com/Rilton/IdentityServer4.Samples.Mongo/blob/dd2fc4f2e9d375c941db5c6042e1e2132ac8209c/9_MongoRepository/src/QuickstartIdentityServer/Quickstart/Extension/IdentityServerBuilderExtensions.cs). A quick
look at those classes should shed some light.

- MongoDB connectivity is based on  nugets and  on a simple Repository Design Pattern implementation. See  interface/IRepository & /interface/MongoRepository classes - everything comes togheter with .NET Core Dependency Injection (see startup.cs & IdentityServerBuilderExtensions.cs)  

#### Configuring Mongo 
For simplicity we recommend to try to use MongoDB locally first, 
so you will be able to have this sample running in no time. 
If you do NOT wish to run mongo locally, it is just a matter of adjusting
the "appsettings.json" file under QuickstartIdentityServer project.
 
First we need to download / install mongo from [https://www.mongodb.com](https://www.mongodb.com) 

Secondly create a folder to store the database and tell mongo to start a new
process inside this folder. It also stores the command to start the database
inside a file called `start.bat` which you can use again to restart the
database when needed. - This is a quick / simple approach to run Mongo locally.

From your command prompt, execute the following commands:
```
>cd C:\
>mkdir mongodb
>cd mongodb
>mkdir identity4db
>cd identity4db
>@echo "C:\Program Files\MongoDB\Server\3.4\bin\mongod.exe" --dbpath "C:\mongodb\identity4db" > identity4db.bat
>identity4db.bat
```

With your local database in place, you may wish use [Robomongo](http://robomongo.org/)
to browse/ edit its content. 
 
 
#### First execution 
Make sure you have multiple projects selected to startup - "QuickstartIdentityServer", "Api" and any desired client (e.g. Clients\MvcClient) before executing solution from Visual Studio.

The first execution will create a  new Mongo  Repository (database), after that due to MongoDB.Driver limitations it is necessary to restart the website in order to proper configure Mongo to ignore Extra Elements such as  "_id" that does not exist in IdentityServer4.Models classes.
