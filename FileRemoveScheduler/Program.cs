using System;
using PeterKottas.DotNetCore.WindowsService;

namespace FileRemoveScheduler
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceRunner<FileRemoveSchedulerMicroService>.Run(config =>
            {
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) => new FileRemoveSchedulerMicroService());

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        Console.WriteLine("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnShutdown(service => { Console.WriteLine("Service {0} shutdown", name); });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });       
        }
    }

}
