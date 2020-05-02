using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserRegistrationWorkerService.Workers;

namespace UserRegistrationWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure the workers that will be responsible for processing the income tasks
                    services.AddHostedService<PersistUserWorker>();
                    services.AddHostedService<SendEmailWorker>();
                });
    }
}
