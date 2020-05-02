using Camunda.Api.Client;
using Camunda.Api.Client.ProcessDefinition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserRegistrationApi.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> UserRegistration(RegistrationModel request)
        {
            _logger.LogInformation("Registering new user...");
            _logger.LogInformation($"name: {request.Name}");

            try
            {
                // Starting camunda client
                CamundaClient camunda = CamundaClient.Create("http://localhost:8080/engine-rest");

                // New process defintion
                StartProcessInstance newProcessInstance = new StartProcessInstance();
                newProcessInstance.SetVariable("name", VariableValue.FromObject(request.Name));
                newProcessInstance.SetVariable("password", VariableValue.FromObject(request.Password));

                // Sending process to camunda
                await camunda.ProcessDefinitions.ByKey("user_registration").StartProcessInstance(newProcessInstance);

                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling user registration for user {request.Name}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
