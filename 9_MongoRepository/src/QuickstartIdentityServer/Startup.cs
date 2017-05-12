using System;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using QuickstartIdentityServer.Quickstart.Extension;
using Serilog;

namespace QuickstartIdentityServer
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }


        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment env)
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

            //--- Configure Serilog ---
            var serilog = new LoggerConfiguration()
                                .ReadFrom.Configuration(Configuration);

            loggerFactory.WithFilter(new FilterLoggerSettings
                        {
                            { "IdentityServer", LogLevel.Error },
                            { "Microsoft", LogLevel.Error },
                            { "System", LogLevel.Error },
                        })
                        .AddSerilog(serilog.CreateLogger());
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddMvc();

            // Dependency Injection - Register the IConfigurationRoot instance mapping to our "ConfigurationOptions" class 
            services.Configure<ConfigurationOptions>(Configuration);


            // ---  configure identity server with in-memory stores, keys, clients and scopes ---
            //services.AddIdentityServer()
            //    .AddTemporarySigningCredential()
            //    .AddInMemoryIdentityResources(Config.GetIdentityResources())
            //    .AddInMemoryApiResources(Config.GetApiResources())
            //    .AddInMemoryClients(Config.GetClients())
            //    .AddTestUsers(Config.GetUsers());


            //User Mongodb for Asp.net identity in order to get users stored.
            //    Using an instance of ConfigurationOptions (Dependency Injection)
            var configurationOptions = Configuration.Get<ConfigurationOptions>();
            var client = new MongoClient(configurationOptions.MongoConnection);  //new MongoClient(Configuration["MongoConnection"]);
            var db = client.GetDatabase(configurationOptions.MongoDatabaseName);  //client.GetDatabase(Configuration["MongoDatabaseName"]);
            services.AddMongoDbForAspIdentity<IdentityUser, IdentityRole>(db);

          


            // ---  configure identity server with MONGO Repository for stores, keys, clients and scopes ---
            services.AddIdentityServer(
                    // Enable IdentityServer events for logging capture - Events are not turned on by default
                    options =>
                    {
                        options.Events.RaiseSuccessEvents = true;
                        options.Events.RaiseFailureEvents = true;
                        options.Events.RaiseErrorEvents = true;
                    }
                )
                .AddTemporarySigningCredential()
                .AddMongoRepository()
                .AddClients()
                .AddIdentityApiResources()
                .AddPersistedGrants()
                .AddTestUsers(Config.GetUsers())
                .AddProfileService<ProfileService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(@"d:\tempidentityserver4_log.txt");

            loggerFactory
                .AddSerilog(serilog.CreateLogger());

            loggerFactory.AddConsole(LogLevel.Debug);
            app.UseDeveloperExceptionPage();

            app.UseIdentity();

            app.UseIdentityServer();

            app.UseMongoDbForIdentityServer();
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
        }
    }
}