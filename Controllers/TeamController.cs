using Sarkaar_Apis.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly SarkaarDbContext _context;
    private readonly IConfiguration _configuration;
    public TeamController(SarkaarDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


    [HttpGet("bycode/{gameCode}")]
    public async Task<IActionResult> GetTeamsByCode(string gameCode)
    {
        var teams = await _context.Teams
            .Where(t => t.GameCode == gameCode)
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                GameCode = t.GameCode
            })
            .ToListAsync();
        return Ok(teams);
    }

    [HttpPost("create")]
    // [Authorize]
    public async Task<IActionResult> CreateTeam([FromBody] TeamCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 3)
            return BadRequest("Team name must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(dto.GameCode))
            return BadRequest("Game code is required.");
        if (!dto.Balance.HasValue || dto.Balance < 0)
            return BadRequest("Initial balance must be a non-negative number.");
        // Only store the team name and game code
        var team = new Team
        {
            Name = dto.Name,
            GameCode = dto.GameCode,
            Balance = dto.Balance.Value
        };
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Team created successfully." });
    }
    [HttpDelete("bycode/{gameCode}")]
    public async Task<IActionResult> DeleteTeamsByCode(string gameCode)
    {
        var teams = _context.Teams.Where(t => t.GameCode == gameCode);
        _context.Teams.RemoveRange(teams);

        // Remove all GameControls for this game code
        var controls = _context.GameControls.Where(c => c.GameCode == gameCode);
        _context.GameControls.RemoveRange(controls);

        // Remove all ChatMessages for this game code
        var chatMessages = _context.ChatMessages.Where(m => m.RoomCode == gameCode);
        _context.ChatMessages.RemoveRange(chatMessages);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Teams, controls, and chats deleted successfully." });
    }
    [HttpPut("update-balance/{teamId}")]
    public async Task<IActionResult> UpdateBalance(int teamId, [FromBody] decimal newBalance)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null)
            return NotFound("Team not found.");

        team.Balance = newBalance;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Balance updated successfully." });
    }
}