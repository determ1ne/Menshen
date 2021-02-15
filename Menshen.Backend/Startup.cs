using System;
using Menshen.Backend.Migrations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Menshen.Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (string.IsNullOrWhiteSpace(_configuration["SQLITE_DB_PATH"]))
                throw new Exception("Invalid sqlite db file path.");
            services.AddSqliteAppDb(_configuration["SQLITE_DB_PATH"]);
            services.AddSqliteMetaInfo();
            services.AddSqliteLocker();
            
            services.AddControllers();
#if DEBUG
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Menshen.Backend", Version = "v1"});
            });
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
#if DEBUG
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Menshen.Backend v1"));
            }
#endif  

            if (_configuration["HTTPS_REDIRECTION"].ToLowerInvariant() == "true")
                app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            var migrationManager = new MigrationManager(serviceProvider);
            migrationManager.Migrate();
        }
    }
}