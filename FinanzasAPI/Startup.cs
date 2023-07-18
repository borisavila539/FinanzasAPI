using Infraestructure.Data;
using Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using FinanzasAPI.Features.Repositories;

namespace FinanzasAPI
{
    public class Startup
    {
        readonly string CorsOrigins = "AllowAnyCorsPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: CorsOrigins,
                                    builder =>
                                    {
                                        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                                    });
            });

            services.AddControllers();
            services.AddDbContext<AxContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("MicrosoftDynamicsAX_PRO"))
            );

            services.AddTransient<INotificacionPagoProveedorRepository, NotificacionPagoRepository>();
            services.AddTransient<IPantsQualityRepository, PantsQualityRepository>();
            services.AddTransient<IAudiTelasRepository, AuditelasRepository>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinanzasAPI", Version = "v1" });
            });
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinanzasAPI v1"));
            }


            app.UseHttpsRedirection();
            
            app.UseCors(CorsOrigins);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
