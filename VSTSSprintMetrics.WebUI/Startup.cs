using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using VSTSSprintMetrics.Cache;
using VSTSSprintMetrics.Core.Interfaces;
using VSTSSprintMetrics.Core.Interfaces.Repositories;
using VSTSSprintMetrics.Core.Interfaces.Services;
using VSTSSprintMetrics.Core.Services;
using VSTSSprintMetrics.VSTSClient;
using VSTSSprintMetrics.VSTSClient.Repositories;

namespace VSTSSprintMetrics.WebUI
{
    public class Startup
    {
        private bool mProductionEnv;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            services.AddMvc(options =>
            {
                if(mProductionEnv)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddScoped<IVSTSTeamsRepository, VSTSTeamsRepository>();
            services.AddScoped<IVSTSIterationsRepository, VSTSIterationsRepository>();
            services.AddScoped<IVSTSWorkItemsRepository, VSTSWorkItemsRepository>();
            services.AddScoped<IMetricsService, MetricsService>();
            services.AddScoped<IDataCache, AzureTableStorageDataCache>();

            services.AddHttpClient("VSTS", client =>
            {
                var options = Configuration.GetSection("VSTSAPI").Get<VSTSApiSettings>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", options.AccessToken);

            });

            services.AddHttpClient<IVSTSTeamsRepository, VSTSTeamsRepository>("VSTS");
            services.AddHttpClient<IVSTSIterationsRepository, VSTSIterationsRepository>("VSTS");
            services.AddHttpClient<IVSTSWorkItemsRepository, VSTSWorkItemsRepository>("VSTS");

            services.Configure<VSTSApiSettings>(Configuration.GetSection("VSTSAPI"));
            services.Configure<StorageAccountSettings>(Configuration.GetSection("StorageAccount"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            mProductionEnv = env.IsProduction();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
