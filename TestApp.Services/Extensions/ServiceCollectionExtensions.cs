using Microsoft.Extensions.DependencyInjection;

namespace TestApp.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services.AddSingleton<IModelConverterFactory, ModelConverterFactory>();
        }
    }
}
