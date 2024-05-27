using OrderApi.DataLayer.Implementations;
using OrderApi.DataLayer.Interfaces;

namespace OrderApi
{
    public static class AppBuilder
    {
        public static WebApplication Create(string[] args, bool isForTests = false)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHostedService<Worker>();
            
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IOrderQueue, MemoryOrderQueue>();
            builder.Services.AddTransient<IOrderAggregator, OrderAggregator>();
            builder.Services.AddTransient<IOrderPublisher, OrderPublisher>();

            // built in way to validate scopes on Build()
            builder.Host.UseDefaultServiceProvider((_, serviceProviderOptions) =>
            {
                serviceProviderOptions.ValidateScopes = true;
                serviceProviderOptions.ValidateOnBuild = true;
            });

            if (isForTests)
            {
                builder.Services.AddTransient<Worker>();
            }

            return builder.Build();
        }
    }
}
