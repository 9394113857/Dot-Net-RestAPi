using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace firstapp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(); // Add MVC services to the application
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Show detailed error page in development environment
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); // Use a custom error page in production environment
                app.UseHsts(); // Enable HTTPS Strict Transport Security (HSTS)
            }

            app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
            app.UseStaticFiles(); // Enable serving static files (e.g., CSS, JavaScript, images)

            app.UseRouting(); // Enable routing

            app.UseAuthorization(); // Enable authorization

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"); // Configure default route
            });
        }
    }
}
