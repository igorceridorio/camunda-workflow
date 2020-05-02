using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace UserRegistrationApi.Configurations
{
    public class SwaggerConfiguration
    {
        public static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Camunda Workflow - User Registration", Version = "v1" });
            });
        }
    }
}
