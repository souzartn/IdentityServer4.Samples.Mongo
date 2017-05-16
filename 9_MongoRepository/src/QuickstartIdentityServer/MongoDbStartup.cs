using System;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QuickstartIdentityServer.Quickstart.Interface;

namespace QuickstartIdentityServer
{
    public static class MongoDbStartup
    {
        private static string  _newRepositoryMsg = $"Mongo Repository created/populated! Please restart your website, so Mongo driver will be configured  to ignore Extra Elements.";
      
        /// <summary>
        /// Adds the support for MongoDb Persistance for all identityserver stored
        /// </summary>
        /// <remarks><![CDATA[
        /// It implements and used mongodb collections for:
        /// - Clients
        /// - IdentityResources
        /// - ApiResource
        /// ]]></remarks>
        public static void UseMongoDbForIdentityServer(this IApplicationBuilder app)
        {
            //Resolve Repository with ASP .NET Core DI help 
            var repository = app.ApplicationServices.GetService<IRepository>();

            // --- Configure Classes to ignore Extra Elements (e.g. _Id) when deserializing ---
            ConfigureMongoDriver2IgnoreExtraElements();

            var createdNewRepository = false;


            //  --Client
            if (!repository.CollectionExists<Client>())
            {
                foreach (var client in Config.GetClients())
                {
                    repository.Add(client);
                }
                createdNewRepository = true;
            }

            //  --IdentityResource
            if (!repository.CollectionExists<IdentityResource>())
            {
                foreach (var res in Config.GetIdentityResources())
                {
                    repository.Add(res);
                }
                createdNewRepository = true;
            }


            //  --ApiResource
            if (!repository.CollectionExists<ApiResource>())
            {
                foreach (var api in Config.GetApiResources())
                {
                    repository.Add(api);
                }
                createdNewRepository = true;
            }

            // If it's a new Repository (database), need to restart the website to configure Mongo to ignore Extra Elements.
            if (createdNewRepository)
            {
                 throw new Exception(_newRepositoryMsg);
            }

            // --- The following will do the initial DB population (If needed / first time) ---
            InitializeDatabase( repository);
        }

        /// <summary>
        /// Configure Classes to ignore Extra Elements (e.g. _Id) when deserializing
        /// As we are using "IdentityServer4.Models" we cannot add something like "[BsonIgnore]"
        /// </summary>
        private static void ConfigureMongoDriver2IgnoreExtraElements()
        {
            BsonClassMap.RegisterClassMap<Client>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<IdentityResource>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<ApiResource>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
            BsonClassMap.RegisterClassMap<PersistedGrant>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        private static  void InitializeDatabase(IRepository repository)
        {
            var createdNewRepository = false;

            //  --Client
            if (!repository.CollectionExists<Client>())
            {
                foreach (var client in Config.GetClients())
                {
                    repository.Add(client);
                }
                createdNewRepository = true;
            }

            //  --IdentityResource
            if (!repository.CollectionExists<IdentityResource>())
            {
                foreach (var res in Config.GetIdentityResources())
                {
                    repository.Add(res);
                }
                createdNewRepository = true;
            }


            //  --ApiResource
            if (!repository.CollectionExists<ApiResource>())
            {
                foreach (var api in Config.GetApiResources())
                {
                    repository.Add(api);
                }
                createdNewRepository = true;
            }

            // If it's a new Repository (database), need to restart the website to configure Mongo to ignore Extra Elements.
            if (createdNewRepository)
            {
                throw new Exception(_newRepositoryMsg);
            }
        }
    }
}