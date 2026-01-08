using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sarkaar_Apis.Models;
using Sarkaar_Apis.Dtos;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidController : ControllerBase
    {
        private readonly SarkaarDbContext _context;
        private readonly IHubContext<SarkaarRoomHub> _hubContext;
        public BidController(SarkaarDbContext context, IHubContext<SarkaarRoomHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BidDto>>> GetBids()
        {
            var bids = await _context.Bids.Select(b => new BidDto
            {
                Id = b.Id,
                TeamId = b.TeamId,
                Amount = b.Amount,
                IsActive = b.IsActive,
                GameId = b.GameId,
                CreatedAt = b.CreatedAt
            }).ToListAsync();
            return Ok(bids);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BidDto>> GetBid(int id)
        {
            var bid = await _context.Bids.FindAsync(id);
            if (bid == null) return NotFound();
            return Ok(new BidDto
            {
                Id = bid.Id,
                TeamId = bid.TeamId,
                Amount = bid.Amount,
                IsActive = bid.IsActive,
                GameId = bid.GameId,
                CreatedAt = bid.CreatedAt
            });
        }

        [HttpPost]
        public async Task<ActionResult<BidDto>> CreateBid(CreateBidDto dto)
        {
            var bid = new Bid
            {
                TeamId = dto.TeamId,
                Amount = dto.Amount,
                GameId = dto.GameId
            };
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();
            // Broadcast bid to all teams in the room using SignalR
            await _hubContext.Clients.Group((bid.GameId ?? 0).ToString()).SendAsync("BidReceived", new { gameId = bid.GameId, teamId = bid.TeamId, amount = bid.Amount });
            return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, new BidDto
            {
                Id = bid.Id,
                TeamId = bid.TeamId,
                Amount = bid.Amount,
                IsActive = bid.IsActive,
                GameId = bid.GameId,
                CreatedAt = bid.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBid(int id)
        {
            var bid = await _context.Bids.FindAsync(id);
            if (bid == null) return NotFound();
            _context.Bids.Remove(bid);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
