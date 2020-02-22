#region snippet_UsingUsersApiModels
using Auth.Api.Infrastructure.Helpers;
using Auth.Api.Models;
#endregion
#region using
using Auth.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using Auth.Api.Infrastructure.ServiceDiscovery;
using Polly;
using System.Net.Http;
using System;
using Polly.Extensions.Http;
using Auth.Api.Infrastructure.Handlers;
#endregion

namespace Auth.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private string gatewayBaseURL = string.Empty;

        #region ConfigureServices
        public void ConfigureServices(IServiceCollection services)
        {
            #region "Cors"
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            #endregion

            #region MongoSetting
            services.Configure<UserstoreDatabaseSettings>(
                Configuration.GetSection(nameof(UserstoreDatabaseSettings)));

            services.AddSingleton<IUserstoreDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<UserstoreDatabaseSettings>>().Value);

            services.AddSingleton<UserService>();
            #endregion

            #region Service Discovery
            ConfigureConsul(services);
            #endregion

            #region CircuitBreaker
            gatewayBaseURL = Configuration["GatewayBaseURL"];

            services.AddHttpClient("gateway", c =>
            {
                c.BaseAddress = new Uri(gatewayBaseURL);
            })
           .AddHttpMessageHandler<AccessTokenHttpMessageHandler>()
           .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.CircuitBreakerAsync(
               handledEventsAllowedBeforeBreaking: 2,
               durationOfBreak: TimeSpan.FromMinutes(1)
           ));
            #endregion

            #region AppSetting
            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            #endregion

            #region Mvc
            services.AddMvc()
                    .AddJsonOptions(options => options.UseMemberCasing())
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            #endregion

            #region Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Auth API v1", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Add \"Bearer\" before the token",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });
            });
            #endregion
        }
        #endregion

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger();
            app.UseCors("CorsPolicy");
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Api v1");
            });
            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void ConfigureConsul(IServiceCollection services)
        {
            var serviceConfig = Configuration.GetServiceConfig();

            services.RegisterConsulServices(serviceConfig);
        }        
    }
}
