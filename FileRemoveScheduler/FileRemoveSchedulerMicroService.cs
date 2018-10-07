using System;
using System.IO;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.Extensions.Configuration;
using PeterKottas.DotNetCore.WindowsService.Interfaces;
using Serilog;

namespace FileRemoveScheduler
{
    public class FileRemoveSchedulerMicroService : IMicroService
        {
            public static void JobToRun()
            {
                IConfigurationRoot configuration = GetConfiguration();
                var folderLocation = configuration.GetSection("folderLocation").Value;
                DirectoryInfo topDircetory = new DirectoryInfo(folderLocation);
                Console.WriteLine($"Checking if files need deleting now {DateTime.Now}");

                var tenDaysAgoToday =
                    DateTime.UtcNow.AddDays(-int.Parse(configuration.GetSection("retentionDays").Value));
                foreach (var item in topDircetory.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    if (item.LastWriteTimeUtc <= tenDaysAgoToday)
                    {
                        try
                        {
                            item.Delete();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error deleting file");
                        }
                    }
                }
            }

            private static IConfigurationRoot GetConfiguration()
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                return configuration;
            }

            public void Start()
            {
                try
                {
                    Log.Logger = new LoggerConfiguration().WriteTo.File("FileRemoveSchedulerMicroService.log").CreateLogger();
                    GlobalConfiguration.Configuration.UseLiteDbStorage("FileRemoveSchedulerMicroService.db");
                    using (new BackgroundJobServer())
                    {
                        RecurringJob.AddOrUpdate(() => JobToRun(), Cron.Daily);
                        Console.ReadLine();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            public void Stop()
            {
                Console.WriteLine("FileRemoveSchedulerMicroService Service stopped.");
            }
        }
    
}