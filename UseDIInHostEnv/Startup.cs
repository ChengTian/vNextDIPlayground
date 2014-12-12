using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.ConfigurationModel;

namespace UseDIInHostEnv
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new Configuration();
            configuration.AddEnvironmentVariables();
            services.AddInstance<IConfiguration>(configuration);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Computer name: " + app.ApplicationServices.GetService<IConfiguration>().Get("COMPUTERNAME"));
            });
        }
    }
}
