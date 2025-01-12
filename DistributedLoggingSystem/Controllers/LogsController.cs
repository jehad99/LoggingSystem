using DistributedLoggingSystem.Models;
using DistributedLoggingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedLoggingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly BatchLogService _batchLogService;
        private readonly LoggingDbContext _context;

        public LogsController(BatchLogService batchLogService, LoggingDbContext context)
        {
            _batchLogService = batchLogService;
            _context = context;
        }

        [HttpPost("add")]
        public IActionResult AddLog([FromBody] Log Log)
        {
            _batchLogService.AddLog(Log);
            return Ok();
        }

        [HttpGet("range")]
        public async Task <IActionResult> GetLogsByRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate,[FromQuery] int page = 1)
        {
            var logs = await  _batchLogService.GetLogsByRange(startDate, endDate, page);

            // need to return the logs list here as a response to the client

            return Ok(logs);

        }
    }
}
