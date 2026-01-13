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
        [HttpGet("by-game/{gameId}")]
        public async Task<ActionResult<IEnumerable<BidDto>>> GetBidsByGame(int gameId)
        {
            var bids = await _context.Bids
                .Where(b => b.GameId == gameId)
                .Select(b => new BidDto
                {
                    Id = b.Id,
                    TeamId = b.TeamId,
                    Amount = b.Amount,
                    IsActive = b.IsActive,
                    GameId = b.GameId,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(bids);
        }


        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<BidDto>> CreateBid(CreateBidDto dto)
        {
            var existingBid = await _context.Bids
                .FirstOrDefaultAsync(b =>
                    b.TeamId == dto.TeamId &&
                    b.GameId == dto.GameId &&
                    b.IsActive);

            if (existingBid != null)
            {
                existingBid.Amount = dto.Amount;
                existingBid.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                existingBid = new Bid
                {
                    TeamId = dto.TeamId,
                    Amount = dto.Amount,
                    GameId = dto.GameId,
                    IsActive = true
                };
                _context.Bids.Add(existingBid);
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients
                .Group(dto.GameId.ToString())
                .SendAsync("BidReceived", new
                {
                    gameId = dto.GameId,
                    teamId = dto.TeamId,
                    amount = dto.Amount
                });

            return Ok(new BidDto
            {
                Id = existingBid.Id,
                TeamId = existingBid.TeamId,
                Amount = existingBid.Amount,
                IsActive = existingBid.IsActive,
                GameId = existingBid.GameId,
                CreatedAt = existingBid.CreatedAt
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
