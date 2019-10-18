using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetroGamingWebAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NSwag.AspNetCore;

namespace RetroGamingWebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RetroGamingContext>(options => {
                options.UseInMemoryDatabase(databaseName: "RetroGaming");
            });

            services.AddTransient<IMailService, MailService>();

            services.AddOpenApiDocument(document =>
            {
                document.DocumentName = "v1";
                document.PostProcess = d => d.Info.Title = "Retro Gaming Web API v1.0 OpenAPI";
            });

            services
                .AddControllers(options => {
                    options.RespectBrowserAcceptHeader = true;
                    options.ReturnHttpNotAcceptable = true;
                    options.FormatterMappings.SetMediaTypeMappingForFormat("xml", new MediaTypeHeaderValue("application/xml"));
                    options.FormatterMappings.SetMediaTypeMappingForFormat("json", new MediaTypeHeaderValue("application/json"));
                })
                .AddNewtonsoftJson(setup => {
                    setup.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddXmlSerializerFormatters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            RetroGamingContext context)
        {
            app.UseOpenApi(config =>
            {
                config.DocumentName = "v1";
                config.Path = "/openapi/v1.json";
            });

            if (env.IsDevelopment())
            {
                DbInitializer.Initialize(context).Wait();
                app.UseStatusCodePages();
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUi3(config =>
                {
                    config.SwaggerRoutes.Add(new SwaggerUi3Route("v1.0", "/openapi/v1.json"));

                    config.Path = "/openapi";
                    config.DocumentPath = "/openapi/v1.json";
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}