using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using Unchase.FluentPerformanceMeter.AspNetCore.Mvc.Extensions;
using Unchase.FluentPerformanceMeter.TestWebAPI31.Controllers;
using Unchase.FluentPerformanceMeter.TestWebAPI31.OpenApiExamples;

namespace Unchase.FluentPerformanceMeter.TestWebAPI31
{
#pragma warning disable CS1591
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddPerformanceDiagnosticObserver<PerformanceMeterController>();

            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.StringEscapeHandling = StringEscapeHandling.Default;
            }).SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Unchase.PerformanceMeter Test WebAPI (.NET Core 3.1)",
                    Version = "v1",
                    Description = "Unchase.PerformanceMeter Test WebAPI (.NET Core 3.1)",
                    License = new OpenApiLicense
                    {
                        Name = "Apache-2.0",
                        Url = new Uri("https://github.com/unchase/Unchase.PerformanceMeter/blob/master/LICENSE.md")
                    }
                });

                // add OpenApi filters with examples
                c.ExampleFilters();

                // add documentation from xml-file comments from assemblies
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.IsDynamic)
                    {
                        var xmlFile = $"{assembly.GetName().Name}.xml";
                        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                        if (File.Exists(xmlPath))
                            c.IncludeXmlComments(xmlPath);
                    }
                }

                // add OpenApi Annotations
                c.EnableAnnotations();

                // sort actions by routeValue and relativePath
                c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");
            });

            services.AddSwaggerGenNewtonsoftSupport();

            services.AddSwaggerExamplesFromAssemblyOf<ResponseExamples>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Unchase.PerformanceMeter Test WebAPI (.NET Core 3.1)";
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Unchase.PerformanceMeter Test WebAPI v1 (.NET Core 3.1)");
                c.ConfigObject.DisplayRequestDuration = true;
            });

            app.UsePerformanceDiagnosticObserver();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
#pragma warning restore CS1591
}
