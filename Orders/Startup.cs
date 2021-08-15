using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Orders.Persistance;

namespace Orders
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
            services.AddControllers();

            var db = new DataContext(new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(Configuration.GetConnectionString("WebContext")).Options);
            services.AddScoped<Persistance.Interfaces.IOrderStorage, Persistance.EF.OrderStorage>(
                _ => new Persistance.EF.OrderStorage(db));
            services.AddScoped<Persistance.Interfaces.IProductStorage, Persistance.EF.ProductStorage>(
                _ => new Persistance.EF.ProductStorage(db));
            services.AddScoped<Persistance.Interfaces.IProductDiscountsStorage, Persistance.EF.ProductDiscountStorage>(
                _ => new Persistance.EF.ProductDiscountStorage(db));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders", Version = "v1" });
            });
            services.AddControllers(options =>
                options.OutputFormatters.Insert(0, new Formatters.OrderFormatter()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders v1"));
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
