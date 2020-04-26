using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TMenos3.NetCore.Serilog.API.Controllers
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
        private readonly Random _random = new Random();

        public WeatherForecastController(ILogger<WeatherForecastController> logger) =>
            _logger = logger;

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var complexObject = new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = 25
            };
            _logger.LogInformation(
                "This is an example of a complex object: {@ComplexObject}.", complexObject);

            CustomOperation();

            _logger.LogInformation(
                "This log should not contain extra properties");

            return CreateResponse();
        }

        private void CustomOperation()
        {
            using var scope = _logger.BeginScope("Extra properties {@A} and {@B}", "A", "B");
            _logger.LogInformation("Inner scope 1.");

            using (var scope2 = _logger.BeginScope("{@C}", "C"))
            {
                _logger.LogInformation("Inner scope 2.");
            }

            _logger.LogInformation("Inner scope 3.");
        }

        private IEnumerable<WeatherForecast> CreateResponse()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = _random.Next(-20, 55),
                Summary = Summaries[_random.Next(Summaries.Length)]
            });
        }
    }
}
