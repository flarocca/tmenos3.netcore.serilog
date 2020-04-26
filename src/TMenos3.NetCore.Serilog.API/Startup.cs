using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.Elasticsearch;
using System;
using TMenos3.NetCore.Serilog.API.Enrichers;

namespace TMenos3.NetCore.Serilog.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                var nodeUri = _configuration.GetValue<string>("LoggingOptions:NodeUri");

                var loggerConfiguration = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}"
                    )
                    .WriteTo.File("log.txt")
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(nodeUri))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat = $"Logs-{DateTime.Now:yyyy-MM}"
                    })
                    .Enrich.WithMachineName()
                    .Enrich.WithAssemblyName()
                    .Enrich.WithProperty("CustomProperty", "Latino .NET Online")
                    .Enrich.With<CustomEnricher>();

                var logger = loggerConfiguration.CreateLogger();

                loggingBuilder.Services.AddSingleton<ILoggerFactory>(
                        provider => new SerilogLoggerFactory(logger, dispose: false));
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
