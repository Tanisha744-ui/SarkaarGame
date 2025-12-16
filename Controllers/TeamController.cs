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

    [HttpGet]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await _context.Teams
        .Include(t => t.TeamLead)
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name,
                TeamLead = t.TeamLead.Username
            })
            .ToListAsync();

        return Ok(teams);
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateTeam([FromBody] TeamCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 3)
            return BadRequest("Team name must be at least 3 characters.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        int leadUserId = int.Parse(userIdClaim.Value);

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            // await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand("spCreateTeam", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", dto.Name);
                cmd.Parameters.AddWithValue("@TeamLeadId", leadUserId);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
        return Ok(new { message = "Team created successfully." });
    }
}