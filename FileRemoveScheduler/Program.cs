using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace FileRemoveScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot configuration = GetConfiguration();
            Log.Logger = new LoggerConfiguration().WriteTo.File("FileRemoveScheduler.log").CreateLogger();

            var folderLocation = configuration.GetSection("folderLocation").Value;
            DirectoryInfo topDircetory = new DirectoryInfo(folderLocation);
            var tenDaysAgoToday = DateTime.UtcNow.AddDays(-int.Parse(configuration.GetSection("retentionDays").Value));
            
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

            while (true)
            {
                System.Threading.Thread.Sleep(1);
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
    }
}
