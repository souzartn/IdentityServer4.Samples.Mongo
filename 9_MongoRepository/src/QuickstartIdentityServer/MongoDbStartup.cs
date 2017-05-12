using System;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using QuickstartIdentityServer.Quickstart.Interface;

namespace QuickstartIdentityServer
{
    public static class MongoDbStartup
    {
        /// <summary>
        /// Adds mongodb implementation for the asp identity part (saving user and roles)
        /// </summary>
        /// <remarks><![CDATA[
        /// Contains implemenations for
        /// - IUserStore<T>
        /// - IRoleStore<T>
        /// ]]></remarks>
        public static IdentityBuilder AddMongoDbForAspIdentity<TIdentity, TRole>(this IServiceCollection services,
            IMongoDatabase database) where TIdentity : IdentityUser where TRole : IdentityRole
        {
            
            services.AddSingleton<IUserStore<TIdentity>>(x =>
            {
                var usersCollection = database.GetCollection<TIdentity>("Identity_Users");

                IndexChecks.EnsureUniqueIndexOnNormalizedEmail(usersCollection);
                IndexChecks.EnsureUniqueIndexOnNormalizedUserName(usersCollection);

                return new UserStore<TIdentity>(usersCollection);
            });

            services.AddSingleton<IRoleStore<TRole>>(x =>
            {
                var rolesCollection = database.GetCollection<TRole>("Identity_Roles");

                IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

                return new RoleStore<TRole>(rolesCollection);
            });

            return services.AddIdentity<TIdentity, TRole>();
        }

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
            // --- Configure Classes to ignore Extra Elements (e.g. _Id) when deserializing ---
            ConfigureMongoDriver2IgnoreExtraElements();

            var createdNewRepository = false;
            var repository = app.ApplicationServices.GetService<IRepository>();

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
                var newRepositoryMsg =
                    $"Mongo Repository created/populated! Please restart you website, so Mongo driver will be configured  to ignore Extra Elements.";
                throw new Exception(newRepositoryMsg);
            }



            // --- The following will do the initial DB population (If needed / first time) ---
            InitializeDatabase(app);
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

        private static void InitializeDatabase(IApplicationBuilder app)
        {
            var createdNewRepository = false;
            var repository = app.ApplicationServices.GetService<IRepository>();

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
                var newRepositoryMsg =
                    $"Mongo Repository created/populated! Please restart you website, so Mongo driver will be configured  to ignore Extra Elements.";
                throw new Exception(newRepositoryMsg);
            }
        }
    }
}