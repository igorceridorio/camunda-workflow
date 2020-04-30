using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Camunda.Api.Client;
using Camunda.Api.Client.ExternalTask;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Camunda_Workflow.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {

            // Conectando ao motor do Camunda
            CamundaClient camunda = CamundaClient.Create("http://localhost:8080/engine-rest");

            // Conectando ao tópico de Task desejado
            var salvaUsuarioETQ = new ExternalTaskQuery()
            {
                Active = true,
                TopicName = "salvar_usuario"
            };

            // Obtendo todas as Tasks a serem processadas para o tópico desejado
            List<ExternalTaskInfo> salvaUsuarioTasks = await camunda.ExternalTasks.Query(salvaUsuarioETQ).List();

            // ------------------------------------------------------------------------------
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
            // ------------------------------------------------------------------------------
        }
    }
}
