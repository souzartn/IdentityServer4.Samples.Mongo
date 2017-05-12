// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using QuickstartIdentityServer.Quickstart.Extension;
using QuickstartIdentityServer.Quickstart.Interface;
using System;

namespace QuickstartIdentityServer
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
            //    .AddTemporarySigningCredential()
            //    .AddInMemoryIdentityResources(Config.GetIdentityResources())
            //    .AddInMemoryApiResources(Config.GetApiResources())
            //    .AddInMemoryClients(Config.GetClients())
            //    .AddTestUsers(Config.GetUsers());


            // Dependency Injection - Register the IConfigurationRoot instance mapping to our "ConfigurationOptions" class 
            services.Configure<ConfigurationOptions>(Configuration);

            // ---  configure identity server with MONGO Repository for stores, keys, clients and scopes ---
            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddMongoRepository()
                .AddClients()
                .AddIdentityApiResources()
                .AddPersistedGrants()
                .AddTestUsers(Config.GetUsers());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            loggerFactory.AddConsole(LogLevel.Debug);
            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                DisplayName = "Google",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,

                ClientId = "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com",
                ClientSecret = "3gcoTrEDPPJ0ukn_aYYT6PWo"
            });

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

            // --- Configure Classes to ignore Extra Elements (e.g. _Id) when deserializing ---
            ConfigureMongoDriver2IgnoreExtraElements();

            // --- The following will do the initial DB population (If needed / first time) ---
            InitializeDatabase(app);

        }


     

    }
}