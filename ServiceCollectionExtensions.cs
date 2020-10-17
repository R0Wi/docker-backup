using System;
using Microsoft.Extensions.DependencyInjection;

namespace DockerBackup
{
    public static class ServiceCollectionExtensions
{
    public static void AddFactory<TService, TImplementation>(this IServiceCollection services) 
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddTransient<Func<TService>>(x => () => x.GetService<TService>());
    }
}
}