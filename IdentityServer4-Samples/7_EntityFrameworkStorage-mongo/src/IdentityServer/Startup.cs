// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using QuickstartIdentityServer;
using QuickstartIdentityServer.Quickstart.Extension;
using QuickstartIdentityServer.Quickstart.Interface;
using System;

namespace IdentityServer
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var environmentVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environmentVar == null)
            {
                environmentVar = env.EnvironmentName;
            }
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentVar}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // ---  configure identity server with in-memory stores, keys, clients and scopes ---
            //services.AddIdentityServer()
            //    .AddDeveloperSigningCredential()
            //    .AddInMemoryIdentityResources(Config.GetIdentityResources())
            //    .AddInMemoryApiResources(Config.GetApiResources())
            //    .AddInMemoryClients(Config.GetClients())
            //    .AddTestUsers(Config.GetUsers());


            // Dependency Injection - Register the IConfigurationRoot instance mapping to our "ConfigurationOptions" class 
            services.Configure<ConfigurationOptions>(Configuration);

            // ---  configure identity server with MONGO Repository for stores, keys, clients and scopes ---
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddMongoRepository()
                .AddClients()
                .AddIdentityApiResources()
                .AddPersistedGrants()
                .AddTestUsers(Config.GetUsers());


            services.AddAuthentication()
              .AddGoogle("Google", options =>
              {
                  options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                  options.ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com";
                  options.ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo";
              })
              .AddOpenIdConnect("oidc", "OpenID Connect", options =>
              {
                  options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                  options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                  options.Authority = "https://demo.identityserver.io/";
                  options.ClientId = "implicit";

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      NameClaimType = "name",
                      RoleClaimType = "role"
                  };
              });


        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();


            // --- Configure Classes to ignore Extra Elements (e.g. _Id) when deserializing ---
            ConfigureMongoDriver2IgnoreExtraElements();

            // --- The following will do the initial DB population (If needed / first time) ---
            InitializeDatabase(app);

        }

        #region Database
        private static void InitializeDatabase(IApplicationBuilder app)
        {
            bool createdNewRepository = false;
            var repository = app.ApplicationServices.GetService<IRepository>();

            //  --Client
            if (!repository.CollectionExists<Client>())
            {
                foreach (var client in Config.GetClients())
                {
                    repository.Add<Client>(client);
                }
                createdNewRepository = true;
            }

            //  --IdentityResource
            if (!repository.CollectionExists<IdentityResource>())
            {
                foreach (var res in Config.GetIdentityResources())
                {
                    repository.Add<IdentityResource>(res);
                }
                createdNewRepository = true;
            }


            //  --ApiResource
            if (!repository.CollectionExists<ApiResource>())
            {
                foreach (var api in Config.GetApiResources())
                {
                    repository.Add<ApiResource>(api);
                }
                createdNewRepository = true;
            }

            // If it's a new Repository (database), need to restart the website to configure Mongo to ignore Extra Elements.
            if (createdNewRepository)
            {
                var newRepositoryMsg = $"Mongo Repository created/populated! Please restart you website, so Mongo driver will be configured  to ignore Extra Elements - e.g. IdentityServer \"_id\" ";
                throw new Exception(newRepositoryMsg);
            }
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

        #endregion

    }
}