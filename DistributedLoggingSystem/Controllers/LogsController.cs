using DistributedLoggingSystem.Dtos;
using DistributedLoggingSystem.Models;
using DistributedLoggingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedLoggingSystem.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly BatchLogService _batchLogService;

        public LogsController(BatchLogService batchLogService)
        {
            _batchLogService = batchLogService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddLog([FromBody] Log log)
        {
            if (log == null || string.IsNullOrEmpty(log.Service) || string.IsNullOrEmpty(log.Level) || string.IsNullOrEmpty(log.Message))
            {
                return BadRequest(new { Message = "Invalid log data. Ensure all fields are provided." });
            }

            try
            {
                await _batchLogService.AddLog(log);
                return Ok(new { Message = "Log added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the log.", Error = ex.Message });
            }
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetLogsByRange([FromQuery]  string service = null, string level = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 10)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return BadRequest(new { Message = "Both startDate and endDate parameters are required." });
            }

            if (startDate > endDate)
            {
                return BadRequest(new { Message = "startDate must be earlier than endDate." });
            }

            try
            {
                // Create a query parameter object
                var queryParameters = new LogQueryParameters
                {
                    Service = service,
                    Level = level,
                    StartTime = startDate,
                    EndTime = endDate
                };

                // Fetch logs
                var logs = await _batchLogService.GetLogs(queryParameters);

                if (logs == null || logs.Count == 0)
                {
                    return NotFound(new { Message = "No logs found for the specified range." });
                }

                return Ok(new
                {
                    Message = "Logs retrieved successfully.",
                    Logs = logs,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = logs.Count // Ideally, fetch total records from backend
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving logs.", Error = ex.Message });
            }
        }
    }
}
