using System;
using AutoMapper;
using Couchbase.Extensions.DependencyInjection;
using CouchbaseRestfulJobPattern.Data;
using CouchbaseRestfulJobPattern.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CouchbaseRestfulJobPattern
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services
                .AddCouchbase(options => Configuration.GetSection("Couchbase").Bind(options))
                .AddCouchbaseBucket<IDefaultBucketProvider>("default")
                .AddAutoMapper();

            services
                .AddTransient<JobRepository>()
                .AddTransient<StarRepository>()
                .AddTransient<JobService>()
                .AddSingleton<JobProcessor>()
                .AddSingleton<JobRecoveryPoller>()
                .AddSingleton<MessageBusService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            var jobProcessor = app.ApplicationServices.GetRequiredService<JobProcessor>();
            jobProcessor.Start();

            var jobRecoveryPoller = app.ApplicationServices.GetRequiredService<JobRecoveryPoller>();
            jobRecoveryPoller.Start();

            var appLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            appLifetime.ApplicationStopping.Register(() =>
            {
                jobProcessor.Dispose();
                jobRecoveryPoller.Dispose();
            });
            appLifetime.ApplicationStopped.Register(() =>
            {
                app.ApplicationServices.GetRequiredService<ICouchbaseLifetimeService>().Close();
            });
        }
    }
}
