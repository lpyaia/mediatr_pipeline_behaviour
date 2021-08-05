using CQRS.WebApi.Application.Features.ProductFeatures.Queries;
using CQRS.WebApi.Domain.Models;
using CQRS.WebApi.Infrastructure;
using CQRS.WebApi.Infrastructure.Context;
using CQRS.WebApi.Infrastructure.Features.ProductFeatures.Queries;
using CQRS.WebApi.PipelineBehaviors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Reflection;

namespace CQRS.WebApi
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
            services.AddDbContext<ApplicationContext>(options =>
                options.UseInMemoryDatabase("inmemorydb"));

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments(string.Format(@"{0}\CQRS.WebApi.xml", System.AppDomain.CurrentDomain.BaseDirectory));
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "CQRS.WebApi",
                });
            });

            #endregion Swagger

            services.AddScoped<IApplicationContext>(provider => provider.GetService<ApplicationContext>());

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddControllers();
            services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
            services.AddMemoryCache();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(
                typeof(IPipelineBehavior<GetAllProductsQuery, IEnumerable<Product>>),
                typeof(MemoryCacheBehavior<GetAllProductsQuery, IEnumerable<Product>>));
            services.AddTransient(
                typeof(IPipelineBehavior<GetProductByIdQuery, Product>),
                typeof(MemoryCacheBehavior<GetProductByIdQuery, Product>));
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

            #region Swagger

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CQRS.WebApi");
            });

            #endregion Swagger

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}