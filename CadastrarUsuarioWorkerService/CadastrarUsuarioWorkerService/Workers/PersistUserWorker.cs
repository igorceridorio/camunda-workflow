using Camunda.Api.Client;
using Camunda.Api.Client.ExternalTask;
using Camunda.Api.Client.ProcessInstance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserRegistrationWorkerService.Configurations;

namespace UserRegistrationWorkerService
{
    public class PersistUserWorker : BackgroundService
    {
        private readonly ILogger<PersistUserWorker> _logger;
        private readonly ServiceConfiguration _serviceConfiguration;
        public static string WORKER_ID = "PersistUserWorker";

        public PersistUserWorker(
            ILogger<PersistUserWorker> logger, 
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
                _logger.LogInformation("PersistUserWorker running at: {time}", DateTimeOffset.Now);

                // Starting camunda client
                CamundaClient camunda = CamundaClient.Create("http://localhost:8080/engine-rest");

                // Fetching available tasks for that topic
                var fetchExternalTasks = new FetchExternalTasks()
                {
                    MaxTasks = 10,
                    WorkerId = WORKER_ID,
                    Topics = new List<FetchExternalTaskTopic>() { new FetchExternalTaskTopic("persist_user", 2000) }
                };

                List<LockedExternalTask> lockedExternalTasks =  await camunda.ExternalTasks.FetchAndLock(fetchExternalTasks);

                // Processing the tasks
                foreach (LockedExternalTask lockedExternalTask in lockedExternalTasks) {

                    // Loading all variables from this task
                    Dictionary<string, VariableValue> taskVariables = lockedExternalTask.Variables;

                    var name = taskVariables["name"];
                    var password = taskVariables["password"];

                    // Process the task as you wish
                    _logger.LogInformation($"Persisting on DB. New user: {name}, password: {password}");

                    // Setting output variables
                    Dictionary<string, VariableValue> outputVariables = new Dictionary<string, VariableValue>
                    {
                        { "user_id", VariableValue.FromObject(VariableValue.FromObject(new Random().Next(1, 100))) }
                    };

                    // Completes task
                    var completeExternalTask = new CompleteExternalTask()
                    {
                        LocalVariables = outputVariables,
                        WorkerId = WORKER_ID,
                    };

                    await camunda.ExternalTasks[lockedExternalTask.Id].Complete(completeExternalTask);
                }
                
                await Task.Delay(_serviceConfiguration.Interval, stoppingToken);
            }
        }
    }
}
