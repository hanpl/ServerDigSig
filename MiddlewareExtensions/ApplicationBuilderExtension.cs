using DigitalSignageSevice.SubscribeTableDependencies;

namespace DigitalSignageSevice.MiddlewareExtensions
{
    public static class ApplicationBuilderExtension
    {
        public static void UseCountersInformationTableDependency(this IApplicationBuilder applicationBuilder, string connectionString)
        {
            var serviceProvider = applicationBuilder.ApplicationServices;
            var service = serviceProvider.GetService<SubscribeInforGateTableDependency>();
            service.SubscribeTableDependency(connectionString);
        }
    }
}
