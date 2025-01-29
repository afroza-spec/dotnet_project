using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/player")]
[ApiController]
[Authorize(Roles = "Player")]
public class PlayerController : ControllerBase
{
    [HttpGet("dashboard")]
    public IActionResult GetPlayerDashboard()
    {
        return Ok(new { message = "Welcome to Player Dashboard" });
    }
}
