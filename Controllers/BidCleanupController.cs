using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidCleanupController : ControllerBase
    {
        private readonly SarkaarDbContext _context;
        public BidCleanupController(SarkaarDbContext context)
        {
            _context = context;
        }

        [HttpDelete("bygame/{gameId}")]
        public async Task<IActionResult> DeleteBidsByGame(int gameId)
        {
            var bids = _context.Bids.Where(b => b.GameId == gameId);
            _context.Bids.RemoveRange(bids);

            // Also remove GameControls for this game if any
            var game = await _context.Teams.FirstOrDefaultAsync(t => t.Id == gameId);
            if (game != null)
            {
                var controls = _context.GameControls.FirstOrDefault(c => c.GameCode == game.GameCode);
                if (controls != null)
                {
                    _context.GameControls.Remove(controls);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Bids and controls deleted for game." });
        }
    }
}
