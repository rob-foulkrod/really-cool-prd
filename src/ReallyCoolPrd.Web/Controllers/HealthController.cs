using Microsoft.AspNetCore.Mvc;

namespace ReallyCoolPrd.Web.Controllers;

/// <summary>
/// Health check endpoint for deployment validation and monitoring.
/// Returns 200 OK if healthy, 500 Internal Server Error if DEPLOYMENT_ERROR is set to 'true'.
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IConfiguration _configuration;

    public HealthController(ILogger<HealthController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    [ProduceResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public IActionResult Get()
    {
        // Check for deployment error flag
        var deploymentError = _configuration["DEPLOYMENT_ERROR"];
        
        if (!string.IsNullOrEmpty(deploymentError) && 
            bool.TryParse(deploymentError, out var isError) && 
            isError)
        {
            _logger.LogError("Health check failed: DEPLOYMENT_ERROR is set to true");
            return StatusCode(StatusCodes.Status500InternalServerError, new { status = "unhealthy", error = "DEPLOYMENT_ERROR is true" });
        }

        _logger.LogInformation("Health check passed");
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
