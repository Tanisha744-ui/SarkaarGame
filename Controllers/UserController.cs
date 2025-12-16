using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;

public class UserController:ControllerBase
{
    private readonly SarkaarDbContext _context;

    public UserController(SarkaarDbContext context)
    {
        _context = context;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto dto)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Username) || dto.Username.Length < 3)
            return BadRequest("Username must be at least 3 characters.");
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            return BadRequest("Invalid email address.");
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters.");

        if (_context.Users.Any(u => u.Email == dto.Email))
            return BadRequest("Email already registered.");
        if (_context.Users.Any(u => u.Username == dto.Username))
            return BadRequest("Username already taken.");

        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null)
            return BadRequest("Invalid role.");

        // Hash password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Role = role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password is required.");

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("YourSuperSecretKeyHere1234567890!@#$");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "")
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        // Set session (if needed, for demonstration)
        HttpContext.Session.SetString("UserId", user.UserId.ToString());
        HttpContext.Session.SetString("Username", user.Username);

        return Ok(new
        {
            message = "Login successful",
            userId = user.UserId,
            role = user.Role?.Name,
            token = jwtToken
        });
    }

    [HttpGet("jwt-lock")]
    [Authorize]
    public IActionResult JwtLock()
    {
    return Ok(new { message = "JWT is valid!", user = User.Identity?.Name });
    }
}