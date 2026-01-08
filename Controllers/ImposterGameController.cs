using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sarkaar_Apis.Dtos;
using Sarkaar_Apis.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImposterGameController : ControllerBase
    {
        private readonly SarkaarDbContext _db;
        public ImposterGameController(SarkaarDbContext db)
        {
            _db = db;
        }

        [HttpGet("turn-info")]
        public async Task<IActionResult> GetTurnInfo(Guid gameId)
        {
            var game = await _db.ImposterGames.FindAsync(gameId);
            if (game == null) return NotFound();

            return Ok(new
            {
                game.CurrentClueTurnIndex,
                game.CurrentVoteTurnIndex,
                game.CluePhaseComplete,
                game.VotePhaseComplete
            });
        }


        [HttpGet("players")]
        public async Task<IActionResult> GetPlayers(Guid gameId)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null) return NotFound();

            var votedPlayerIds = await _db.ImposterVotes
                .Where(v => v.GameId == gameId)
                .Select(v => v.VoterId)
                .ToListAsync();

            return Ok(game.Players.Select(p => new
            {
                p.PlayerId,
                p.Name,
                p.Clue,
                hasVoted = votedPlayerIds.Contains(p.PlayerId)
            }));
        }

        [HttpGet("clues")]
        public async Task<IActionResult> GetClues(Guid gameId)
        {
            var players = await _db.ImposterPlayers
                .Where(p => p.GameId.Equals(gameId))
                .Select(p => new
                {
                    p.PlayerId,
                    p.Name,
                    text = p.Clue ?? ""
                })
                .ToListAsync();

            return Ok(players);
        }

        [HttpPost("register-player")]
        public async Task<IActionResult> RegisterPlayer(RegisterPlayerDTO req)
        {
            var game = await _db.ImposterGames.FindAsync(req.GameId);
            if (game == null)
                return NotFound("Game not found.");

            var exists = await _db.ImposterPlayers
                .AnyAsync(p => p.GameId.Equals(req.GameId) && p.Name == req.Name);

            if (exists)
                return BadRequest("Player name already exists.");

            var player = new ImposterPlayer
            {
                Name = req.Name,
                GameId = req.GameId // Make sure ImposterPlayer.GameId is of type Guid in your model
            };

            _db.ImposterPlayers.Add(player);
            await _db.SaveChangesAsync();

            return Ok(new { player.PlayerId });
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinGame(JoinGameDTO req)
        {
            var game = await _db.ImposterGames.FindAsync(req.GameId);
            if (game == null)
                return NotFound("Game not found.");

            var player = new ImposterPlayer
            {
                Name = req.Name,
                GameId = req.GameId
            };

            _db.ImposterPlayers.Add(player);
            await _db.SaveChangesAsync();

            return Ok(new { player.PlayerId });
        }


        [HttpPost("start")]
        public async Task<IActionResult> StartGame([FromQuery] Guid gameId)
        {
            // 1. Load game with players
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
                return NotFound("Game not found.");

            if (game.Players.Count < 3)
                return BadRequest("At least 3 players required.");

            // 2. RESET PLAYER STATE
            foreach (var p in game.Players)
            {
                p.IsImposter = false; // <--- This ensures only one imposter
                p.Clue = null;
            }

            // 3. RESET GAME STATE
            game.CurrentClueTurnIndex = 0;
            game.CurrentVoteTurnIndex = 0;
            game.CluePhaseComplete = false;
            game.VotePhaseComplete = false;
            game.IsStarted = true;

            // 4. CLEAR OLD VOTES (DB, NOT MEMORY)
            var oldVotes = _db.ImposterVotes.Where(v => v.GameId == gameId);
            _db.ImposterVotes.RemoveRange(oldVotes);

            // 5. PICK RANDOM IMPOSTER
            var rnd = new Random();
            var imposterList = game.Players.ToList();
            var imposter = imposterList[rnd.Next(imposterList.Count)];

            imposter.IsImposter = true;
            game.ImposterId = imposter.PlayerId;

            // 6. SAVE EVERYTHING
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Game started successfully",
                imposterAssigned = true
            });
        }

        [HttpGet("word")]
        public async Task<IActionResult> GetWord(Guid gameId, Guid playerId)
        {
            var player = await _db.ImposterPlayers
                .FirstOrDefaultAsync(p => p.PlayerId == playerId && p.GameId == gameId);

            if (player == null) return NotFound();

            var game = await _db.ImposterGames
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null) return NotFound();

            // ADD THIS CHECK
            if (!game.IsStarted)
                return BadRequest(new { message = "Game has not started yet." });

            return Ok(new
            {
                word = player.IsImposter ? game.ImposterWord : game.CommonWord,
                player.IsImposter
            });
        }

        [HttpPost("submit-clue")]
        public async Task<IActionResult> SubmitClue(ClueRequestDTO req)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == req.GameId);

            if (game == null) return NotFound();

            if (game.CurrentClueTurnIndex >= game.Players.Count)
                return BadRequest("Clue phase already completed.");

            var player = game.Players
                .OrderBy(p => p.PlayerId)
                .ElementAt(game.CurrentClueTurnIndex);

            if (player.PlayerId != req.PlayerId)
                return BadRequest("Not your turn");

            player.Clue = req.Clue;
            game.CurrentClueTurnIndex++;

            if (game.CurrentClueTurnIndex >= game.Players.Count)
                game.CluePhaseComplete = true;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("vote")]
        public async Task<IActionResult> Vote(VoteRequestDTO req)
        {
            var vote = await _db.ImposterVotes
                .FirstOrDefaultAsync(v => v.GameId == req.GameId && v.VoterId == req.VoterId);

            if (vote == null)
            {
                _db.ImposterVotes.Add(new ImposterVote
                {
                    GameId = req.GameId,
                    VoterId = req.VoterId,
                    SuspectId = req.SuspectId
                });
            }
            else
            {
                vote.SuspectId = req.SuspectId;
            }

            await _db.SaveChangesAsync();

            var totalVotes = await _db.ImposterVotes.CountAsync(v => v.GameId == req.GameId);
            var playerCount = await _db.ImposterPlayers.CountAsync(p => p.GameId.Equals(req.GameId));

            if (totalVotes < playerCount)
                return Ok(new { finished = false });

            var votedOut = await _db.ImposterVotes
                .Where(v => v.GameId == req.GameId)
                .GroupBy(v => v.SuspectId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstAsync();

            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == req.GameId);

            game.IsFinished = true;
            game.FinishedAt = DateTime.UtcNow;

            // --- ADD THIS BLOCK ---
            var accusedPlayer = game.Players.FirstOrDefault(p => p.PlayerId == votedOut);
            var imposterPlayer = game.Players.FirstOrDefault(p => p.IsImposter);

            bool imposterCaught = (imposterPlayer != null && accusedPlayer != null && imposterPlayer.PlayerId == accusedPlayer.PlayerId);

            game.Result = $"{imposterCaught.ToString().ToLower()}|{accusedPlayer?.Name ?? ""}|{imposterPlayer?.Name ?? ""}";
            // --- END BLOCK ---

            await _db.SaveChangesAsync();
            return Ok(new { finished = true, votedOut });
        }


        [HttpGet("result")]
        public async Task<IActionResult> GetResult(Guid gameId)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null || !game.IsFinished)
                return BadRequest();

            bool imposterCaught = false;
            string accused = "";
            string imposter = "";

            if (!string.IsNullOrEmpty(game.Result))
            {
                var parts = game.Result.Split('|');
                if (parts.Length == 3)
                {
                    imposterCaught = parts[0] == "true";
                    accused = parts[1];
                    imposter = parts[2];
                }
            }

            var result = new {
                imposterCaught,
                accused,
                imposter
            };

            return Ok(result);
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> Cleanup([FromBody] string lobbyCode)
        {
            var game = await _db.ImposterGames
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null)
                return NotFound();

            var gameId = game.Id;

            // Remove all related data
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM ImposterRoundDecisions WHERE GameId = {0}", gameId);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM ImposterVotes WHERE GameId = {0}", gameId);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM ImposterClues WHERE GameId = {0}", gameId);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM ImposterPlayers WHERE GameId = {0}", gameId);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM ImposterGames WHERE Id = {0}", gameId);

            return Ok();
        }
    }
}
