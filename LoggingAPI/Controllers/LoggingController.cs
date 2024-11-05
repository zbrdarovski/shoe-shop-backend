using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using LoggingAPI.Models;

namespace LoggingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggingController : ControllerBase
    {
        private readonly LogDatabaseService _logDbService;

        public LoggingController(LogDatabaseService logDbService)
        {
            _logDbService = logDbService;
        }

        [HttpPost("postLogs")]
        public async Task<IActionResult> PostLogs([FromServices] LogDatabaseService logDbService, [FromServices] RabbitMQService rabbitMQService)
        {
            var logsFromRabbitMQ = rabbitMQService.GetAllMessages();
            foreach (var logEntry in logsFromRabbitMQ)
            {
                await logDbService.SaveLog(logEntry);
            }

            return Ok("Logs saved to database.");
        }

        [HttpGet("logs/{startDate}/{endDate}")]
        public async Task<IActionResult> GetLogs(DateTime startDate, DateTime endDate)
        {
            var logs = await _logDbService.GetLogs(startDate, endDate);
            return Ok(logs);
        }

        [HttpDelete("clearLogs")]
        public async Task<IActionResult> ClearLogs()
        {
            await _logDbService.DeleteAllLogs();
            return Ok("Logs cleared from the database.");
        }

    }
}
