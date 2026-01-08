using Microsoft.AspNetCore.Mvc;
using SarkaarGame.Models;

namespace SarkaarGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameControlsController : ControllerBase
    {
        private readonly SarkaarDbContext _context;
        public GameControlsController(SarkaarDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SetGameControls([FromBody] GameControls controls)
        {
            _context.GameControls.Add(controls);
            _context.SaveChanges();
            return Ok(controls);
        }

        [HttpGet("{gameCode}")]
        public IActionResult GetGameControls(string gameCode)
        {
            var controls = _context.GameControls.FirstOrDefault(c => c.GameCode == gameCode);
            if (controls == null) return NotFound();
            return Ok(controls);
        }

        [HttpDelete("{gameCode}")]
        public IActionResult DeleteGameControls(string gameCode)
        {
            var controls = _context.GameControls.FirstOrDefault(c => c.GameCode == gameCode);
            if (controls != null)
            {
                _context.GameControls.Remove(controls);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
