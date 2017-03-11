﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using AbcLeaves.BasicMvcClient.Helpers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;

namespace AbcLeaves.BasicMvcClient
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();
            services.Configure<Helpers.GoogleOAuthOptions>(Configuration.GetSection("GoogleOAuth"));
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSingleton<HttpClientHandler>();
            services.AddTransient<IGoogleOAuthHelper, GoogleOAuthHelper>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Authentication:
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationScheme = "GoogleOpenIdConnectCookies",
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });
            var openIdConnectOptions = new OpenIdConnectOptions() {
                AuthenticationScheme = "GoogleOpenIdConnect",
                SignInScheme = "GoogleOpenIdConnectCookies",
                Authority = "https://accounts.google.com",
                ResponseType = OpenIdConnectResponseType.IdToken,
                CallbackPath = new PathString("/signin-oidc"),
                ClientId = Configuration["GoogleOAuth:ClientId"],
                ClientSecret = Configuration["GoogleOAuth:ClientSecret"],
                AutomaticAuthenticate = false,
                AutomaticChallenge = false,
                GetClaimsFromUserInfoEndpoint = false,
                SaveTokens = true,
                UseTokenLifetime = true,
                BackchannelHttpHandler = app.ApplicationServices.GetService<HttpClientHandler>(),
                Events = new OpenIdConnectEvents()
                {
                    OnTicketReceived = context => {
                        context.Properties.IsPersistent = true;
                        context.Properties.AllowRefresh = false;
                        return Task.CompletedTask;
                    }
                }
            };
            openIdConnectOptions.Scope.Clear();
            openIdConnectOptions.Scope.Add("openid");
            openIdConnectOptions.Scope.Add("email");
            app.UseOpenIdConnectAuthentication(openIdConnectOptions);

            app.UseMvc();
        }
    }
}
