namespace Molla.Foundation.DependencyInjection.Infrastructure
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.DependencyInjection;

    public class MvcControllerServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcControllers("*.Feature.*"); 
            serviceCollection.AddMvcControllers("*.Foundation.*"); 
            serviceCollection.AddClassesWithServiceAttribute("*.Feature.*");
            serviceCollection.AddClassesWithServiceAttribute("*.Foundation.*");
        }
    }
}