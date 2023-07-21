using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TimeTracker.Service;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((services) =>
    {
        services.Add(new ServiceDescriptor(typeof(IEntryService), typeof(EntryService), ServiceLifetime.Singleton));
    })
    .Build();

host.Run();