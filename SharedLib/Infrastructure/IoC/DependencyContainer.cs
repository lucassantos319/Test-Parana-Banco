using Microsoft.Extensions.DependencyInjection;
using SharedLib.Domain.Interfaces;
using SharedLib.Domain.Interfaces.Bus;
using SharedLib.Infrastructure.Repositories;

namespace SharedLib.Infrastructure.IoC
{
    public class DependencyContainer
    {
        public static void RegisterService(IServiceCollection services)
        {
            services.AddTransient<IEventBus, RabbitMqClientBus>();
            services.AddTransient<IClientRepository, ClientRepository>();

        }   
    }
}
