
## Configuring Mongo 
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
>@echo "C:\Program Files\MongoDB\Server\3.6\bin\mongod.exe" --dbpath "C:\mongodb\identity4db" > identity4db.bat
>identity4db.bat
```

With your local database in place, you can use Mongo Compass Community or something a bit more user user friendly like [Robo3T](http://robomongo.org/)
to browse/ edit its content. 
 
 ### Alternative option -  Docker
 
If you prefer use Docker, please take a look at [Dockerize MongoDB](https://docs.docker.com/engine/examples/mongodb/)