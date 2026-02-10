using Microsoft.AspNetCore.Mvc;
using Sarkaar_Apis.DTOs;
using Sarkaar_Apis.Models;
using System.Linq;
using Sarkaar_Apis.Hubs;

namespace Sarkaar_Apis.Controllers
{
    [ApiController]
    [Route("api/party")]
    public class PartyController : ControllerBase
    {
        private readonly SarkaarDbContext _context;

        public PartyController(SarkaarDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public IActionResult CreateParty([FromBody] CreatePartyRequestDto request)
        {
            var party = new Party
            {
                PartyCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                HostName = request.HostName
            };

            _context.Parties.Add(party);
            _context.SaveChanges();

            var response = new CreatePartyResponseDto
            {
                PartyCode = party.PartyCode,
                HostName = party.HostName
            };

            return Ok(response);
        }

        [HttpPost("join")]
        public IActionResult JoinParty([FromBody] JoinPartyRequestDto request)
        {
            var party = _context.Parties.FirstOrDefault(p => p.PartyCode == request.PartyCode);
            if (party == null) return NotFound("Party not found");

            // Check if party is locked or game is started (using PartyHub's static dictionaries)
            bool isLocked = false, isStarted = false;
            Sarkaar_Apis.Hubs.PartyHub.GetPartyLockAndStartedState(request.PartyCode, out isLocked, out isStarted);
            if (isLocked)
                return StatusCode(403, new { error = "Party is locked. You cannot join at this time." });
            if (isStarted)
                return StatusCode(403, new { error = "Game has already started. Please wait for the next game!" });

            var player = new Player { Name = request.PlayerName, PartyId = party.PartyId };
            _context.Players.Add(player);
            _context.SaveChanges();

            var response = new JoinPartyResponseDto
            {
                PartyCode = party.PartyCode ?? string.Empty,
                HostName = party.HostName ?? string.Empty,
                Players = party.Players.Select(p => p.Name ?? string.Empty).ToList()
            };

            return Ok(response);
        }

        [HttpGet("{partyCode}")]
        public IActionResult GetPartyDetails(string partyCode)
        {
            var party = _context.Parties.FirstOrDefault(p => p.PartyCode == partyCode);
            if (party == null) return NotFound("Party not found");

            var response = new PartyDetailsDto
            {
                PartyCode = party.PartyCode ?? string.Empty,
                HostName = party.HostName ?? string.Empty,
                Players = party.Players.Select(p => p.Name ?? string.Empty).ToList()
            };

            return Ok(response);
        }
    [HttpDelete("end/{partyCode}")]
    public IActionResult EndGame(string partyCode)
    {
        var party = _context.Parties.FirstOrDefault(p => p.PartyCode == partyCode);
        if (party == null)
        {
            return NotFound("Party not found");
        }
        // Remove all players for this party
        var players = _context.Players.Where(pl => pl.PartyId == party.PartyId).ToList();
        _context.Players.RemoveRange(players);
        // Remove the party itself
        _context.Parties.Remove(party);
        _context.SaveChanges();
        return Ok(new { message = "Party and players deleted" });
    }

    // Remove a single player from a party
    [HttpDelete("{partyCode}/remove-player/{playerName}")]
    public IActionResult RemovePlayer(string partyCode, string playerName)
    {
        var party = _context.Parties.FirstOrDefault(p => p.PartyCode == partyCode);
        if (party == null)
        {
            return NotFound("Party not found");
        }
        var player = _context.Players.FirstOrDefault(p => p.PartyId == party.PartyId && p.Name == playerName);
        if (player == null)
        {
            return NotFound("Player not found in party");
        }
        _context.Players.Remove(player);
        _context.SaveChanges();
        return Ok(new { message = "Player removed from party" });
    }
  }
}