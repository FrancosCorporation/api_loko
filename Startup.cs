using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using Microsoft.Extensions.Options;
using System.Threading;
using condominioApi.Services;
using condominioApi.Models;

namespace condominioApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Console.Write("\nDate Up Server :" + DateTime.UtcNow+ "\n"+"\n");
        }
        
        public IConfiguration Configuration { get; }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
    
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options => {
                options.AddDefaultPolicy(builder => {
                    builder.AllowAnyOrigin();
                });
            });

            //Configurando o acesso ao MongoDb
            services.Configure<CondominioDatabaseSetting>(
                Configuration.GetSection(nameof(CondominioDatabaseSetting))
            );

            //Certificando de que ira existir uma unica instancia
            services.AddSingleton<ICondominioDatabaseSetting>(sp =>
                sp.GetRequiredService<IOptions<CondominioDatabaseSetting>>().Value);

            services.AddSingleton<CondominioService>();
            services.AddSingleton<PaymentService>();


            services.AddControllers();

            //Adiciona o servico de autenticacao atraves do JsonWebToken
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(x => 
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseCors(MyAllowSpecificOrigins);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
