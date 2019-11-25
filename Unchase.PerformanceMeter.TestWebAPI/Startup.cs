using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;

namespace Unchase.PerformanceMeter.TestWebAPI
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
            services.AddHttpContextAccessor();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Unchase.PerformanceMeter Test WebAPI",
                    Version = "v1",
                    Description = "Unchase.PerformanceMeter Test WebAPI",
                    License = new License
                    {
                        Name = "Apache-2.0",
                        Url = "https://github.com/unchase/Unchase.PerformanceMeter/blob/master/LICENSE.md"
                    }
                });

                // подключаем фильтры с примерами для запросов (ответов)
                c.ExampleFilters();

                // подключаем аннотации для swagger'а
                c.EnableAnnotations();

                // сортируем действия контроллеров по имени контроллера и относительного пути
                c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");
            });

            services.AddSwaggerExamplesFromAssemblyOf<SwaggerRequestResponseExamples>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = "Unchase.PerformanceMeter Test WebAPI";
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", $"Unchase.PerformanceMeter Test WebAPI v1");
                c.ConfigObject.DisplayRequestDuration = true;
            });

            app.UseMvc();
        }
    }
}
