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

                // Registering worker to the desired topic
                var persistUserTaskQuery = new ExternalTaskQuery()
                {
                    Active = true,
                    TopicName = "persist_user"
                };

                // Fetching available tasks for that topic
                List<ExternalTaskInfo> persistUserTasks = await camunda.ExternalTasks.Query(persistUserTaskQuery).List();

                // Processing the tasks
                foreach (ExternalTaskInfo persistUserTask in persistUserTasks) {

                    // Loading all variables from this task
                    var executionId = persistUserTask.ExecutionId;
                    //var name = await camunda.Executions[executionId].LocalVariables.Get("name");
                    //var password = await camunda.Executions[executionId].LocalVariables.Get("password");

                    Dictionary<string, VariableValue> allVariables = await camunda.Executions[executionId]
                        .LocalVariables.GetAll();

                    // Process the task as you wish
                    //_logger.LogInformation($"Persisting on DB. New user: {name}, password: {password}");

                    // Setting output variables
                    await camunda.Executions[executionId].LocalVariables.Set("user_id", VariableValue.FromObject(new Random().Next(1, 100)));

                    // Completing task
                    // await camunda.Executions[executionId].
                    

                }
                
                await Task.Delay(_serviceConfiguration.Interval, stoppingToken);
            }
        }
    }
}
