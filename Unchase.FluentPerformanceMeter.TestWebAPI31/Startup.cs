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

            // adds a singleton service to the specified IPerformanceInfo<PerformanceMeterController> with DI
            services.AddSingleton(s => PerformanceMeter<PerformanceMeterController>.PerformanceInfo);
            // ... the same for another classes (controllers)

            services.AddPerformanceMeter<PerformanceMeterController>(options =>
            {
                // ALL of this is optional. You can simply call .AddPerformanceMeter<MeasurableController>() for all defaults
                // Defaults: In-Memory for 5 minutes, everything watched, every user can see

                // excludes a method from performance watching
                //options.ExcludeMethod(nameof(PerformanceMeterController.SimpleWatchingMethodStartWatchingPerformanceAttribute));

                // to control which requests are watched, use the Func<HttpRequest, bool> option:
                //options.ShouldWatching = request => request.HttpContext.User.IsInRole("Dev");

                // allows to add custom data from custom attributes ("MethodCustomDataAttribute", "MethodCallerAttribute") to performance watching
                //options.AddCustomDataFromCustomAttributes = false;

                // allows to use "IgnoreMethodPerformanceAttribute" for excluding from performance watching
                //options.UseIgnoreMethodPerformanceAttribute = false;

                // allows to watch actions performance annotated with special attribute ("WatchingPerformanceAttribute")
                //options.WatchForAnnotatedWithAttributeOnly = false;

                // excludes a path from being watched
                //options.IgnorePath("/some_path");

                // allows to add route path to custom data (with "pm_path" key)
                //options.AddRoutePathToCustomData = false;

                // set cache time for the watched performance results for the controller class
                //options.SetMethodCallsCacheTime(5);

                // adds common custom data (anonymous class) to class performance information
                //options.AddCustomData("Custom anonymous class", new { Name = "Custom Name", Value = 1 });

                // set default exception handler for the controller class
                //options.SetDefaultExceptionHandler((ex) => Debug.WriteLine(ex.Message));

                // adds a scope service of the PerformanceMeter of concrete class for DI
                // use it with ".WithSettingData.CallerFrom(IHttpContextAccessor)"
                //options.RegisterPerformanceMeterScope = false;
            });

            //services.AddPerformanceDiagnosticObserver<PerformanceMeterController>(options =>
            //{
            //    // the same options like in "AddPerformanceMeter<PerformanceMeterController>(options => {...})"
            //});

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

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
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

            //app.UsePerformanceDiagnosticObserver();

            app.UseRouting();

            app.UseEndpoints(c =>
            {
                c.MapControllers();

                // use performance watching for concrete controller
                app.UsePerformanceMeterFor<PerformanceMeterController>();
            });
        }
    }
#pragma warning restore CS1591
}
