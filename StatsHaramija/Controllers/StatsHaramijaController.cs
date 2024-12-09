using Microsoft.AspNetCore.Mvc;
using StatsHaramija.Models;

namespace StatsHaramija.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StatsHaramijaController : ControllerBase
    {
        private readonly StatsRepository _statsRepository;

        public StatsHaramijaController(StatsRepository statsRepository)
        {
            _statsRepository = statsRepository;
        }

        [HttpGet("GetLastEndpoint")]
        public async Task<ActionResult<ApiCallStat>> GetLastCalledEndpoint()
        {
            var lastCalled = await _statsRepository.GetLastCalledAsync();
            if (lastCalled == null)
                return NotFound("No endpoint calls have been recorded.");

            return Ok(lastCalled);
        }

        [HttpGet("GetMostCalledEndpoint")]
        public async Task<ActionResult<ApiCallStat>> GetMostCalledEndpoint()
        {
            var mostCalled = await _statsRepository.GetMostCalledAsync();
            if (mostCalled == null)
                return NotFound("No endpoint calls have been recorded.");

            return Ok(mostCalled);
        }

        [HttpGet("GetCallsPerEndpoint")]
        public async Task<ActionResult<IEnumerable<ApiCallStat>>> GetCallsPerEndpoint()
        {
            var stats = await _statsRepository.GetAllStatsAsync();
            return Ok(stats);
        }

        [HttpPost("PostUpdate")]
        public async Task<IActionResult> UpdateStats([FromBody] ApiCallStat stat)
        {
            if (stat == null || string.IsNullOrWhiteSpace(stat.Endpoint))
            {
                return BadRequest("Invalid request body.");
            }

            await _statsRepository.UpdateStatAsync(stat.Endpoint);
            return Ok();
        }

    }
}