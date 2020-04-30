using Camunda.Api.Client;
using Camunda.Api.Client.ExternalTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserRegistrationWorkerService.Configurations;

namespace UserRegistrationWorkerService.Workers
{
    public class SendEmailWorker : BackgroundService
    {
        private readonly ILogger<SendEmailWorker> _logger;
        private readonly ServiceConfiguration _serviceConfiguration;
        private static string WORKER_ID = "SendEmailWorker";

        public SendEmailWorker(
            ILogger<SendEmailWorker> logger,
            IConfiguration configuration)
        {
            _logger = logger;

            _serviceConfiguration = new ServiceConfiguration();
            new ConfigureFromConfigurationOptions<ServiceConfiguration>(
                configuration.GetSection("ServiceConfiguration"))
                    .Configure(_serviceConfiguration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{WORKER_ID} running at: {DateTimeOffset.Now}");

                // Starting camunda client
                CamundaClient camunda = CamundaClient.Create("http://localhost:8080/engine-rest");

                // Fetching available tasks for that topic
                var fetchExternalTasks = new FetchExternalTasks()
                {
                    MaxTasks = 10,
                    WorkerId = WORKER_ID,
                    Topics = new List<FetchExternalTaskTopic>() { new FetchExternalTaskTopic("send_email", 2000) }
                };

                List<LockedExternalTask> lockedExternalTasks = await camunda.ExternalTasks.FetchAndLock(fetchExternalTasks);

                // Processing the tasks
                foreach (LockedExternalTask lockedExternalTask in lockedExternalTasks)
                {

                    // Loading all variables from this task
                    Dictionary<string, VariableValue> taskVariables = lockedExternalTask.Variables;

                    var userId = taskVariables["user_id"];

                    // Process the task as you wish
                    _logger.LogInformation($"Sending email to user: {userId}");

                    // Completes task
                    var completeExternalTask = new CompleteExternalTask()
                    {
                        WorkerId = WORKER_ID
                    };

                    await camunda.ExternalTasks[lockedExternalTask.Id].Complete(completeExternalTask);
                }

                await Task.Delay(_serviceConfiguration.Interval, stoppingToken);
            }
        }
    }
}
