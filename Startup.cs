using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using first_api.Models;
using first_api.Services;
using Microsoft.Extensions.Options;
using System.Threading;

namespace first_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Thread t = new Thread(initDb);
            t.Start();
        }
        
        public void initDb() {
            string command = "--dbpath db";
            System.Diagnostics.Process.Start("mongod.exe", command);
        }
        public IConfiguration Configuration { get; }
    
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<AlunoDatabaseSetting>(
                Configuration.GetSection(nameof(AlunoDatabaseSetting))
            );

            services.AddSingleton<IAlunoDatabaseSetting>(sp =>
                sp.GetRequiredService<IOptions<AlunoDatabaseSetting>>().Value);

            services.AddSingleton<AlunoService>();

            services.AddControllers();

        }

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
