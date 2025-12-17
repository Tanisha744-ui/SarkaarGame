using Microsoft.AspNetCore.Mvc;
using Sarkaar_Apis.Models;
using Sarkaar_Apis.Dtos;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Sarkaar_Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImposterGameController : ControllerBase
    {
        [HttpGet("turn-info")]
        public IActionResult GetTurnInfo(Guid gameId)
        {
            if (!games.TryGetValue(gameId, out var game))
                return NotFound();

            return Ok(new
            {
                clueTurnIndex = game.CurrentClueTurnIndex,
                voteTurnIndex = game.CurrentVoteTurnIndex,
                cluePhaseComplete = game.CluePhaseComplete,
                votePhaseComplete = game.VotePhaseComplete
            });
        }


        [HttpGet("players")]
        public IActionResult GetPlayers([FromQuery] Guid gameId)
        {
            if (!games.TryGetValue(gameId, out var game))
                return NotFound();
            return Ok(game.Players.Select(p => new
            {
                playerId = p.PlayerId,
                name = p.Name,
                clue = p.Clue,
                hasVoted = game.Votes.ContainsKey(p.PlayerId),
                voteFor = game.Votes.TryGetValue(p.PlayerId, out var v) ? (Guid?)v : null
            }));
        }

        [HttpGet("clues")]
        public IActionResult GetClues([FromQuery] Guid gameId)
        {
            if (!games.TryGetValue(gameId, out var game))
                return NotFound();

            return Ok(game.Players.Select(p => new
            {
                playerId = p.PlayerId,
                name = p.Name,
                clue = p.Clue
            }));
        }

        // In-memory storage for demo purposes
        private static ConcurrentDictionary<Guid, ImposterGame> games = new();

        [HttpPost("register-player")]
        public IActionResult RegisterPlayer([FromBody] RegisterPlayerDTO req)
        {
            if (!games.TryGetValue(req.GameId, out var game))
                return NotFound("Game not found.");

            if (game.Players.Any(p => p.Name == req.Name))
                return BadRequest("Player name already exists in this game.");

            var player = new ImposterPlayer { Name = req.Name };
            game.Players.Add(player);
            return Ok(new { player.PlayerId });
        }
        [HttpPost("create")]
        public IActionResult CreateGame([FromBody] CreateGameDTO req)
        {
            if (req.PlayerNames == null || req.PlayerNames.Length < 3)
                return BadRequest("At least 3 players required.");


            // Expanded word list: [word, imposter hint]
            var wordSets = new[]
            {
                new[] { "apple", "tasty" },
                new[] { "car", "wheel" },
                new[] { "cat", "meow" },
                new[] { "dog", "bark" },
                new[] { "banana", "yellow" },
                new[] { "train", "track" },
                new[] { "book", "read" },
                new[] { "phone", "call" },
                new[] { "computer", "keyboard" },
                new[] { "river", "water" },
                new[] { "mountain", "peak" },
                new[] { "pencil", "write" },
                new[] { "shoe", "foot" },
                new[] { "tree", "leaf" },
                new[] { "cake", "sweet" },
                new[] { "milk", "white" },
                new[] { "sun", "hot" },
                new[] { "moon", "night" },
                new[] { "star", "shine" },
                new[] { "fish", "swim" },
                new[] { "plane", "fly" },
                new[] { "clock", "time" },
                new[] { "rain", "wet" },
                new[] { "ice", "cold" },
                new[] { "bread", "slice" },
                new[] { "ring", "finger" },
                new[] { "shirt", "button" },
                new[] { "glass", "drink" },
                new[] { "mouse", "click" },
                new[] { "camera", "photo" },
                new[] { "bike", "pedal" },
                new[] { "bus", "stop" },
                new[] { "egg", "breakfast" },
                new[] { "chair", "sit" },
                new[] { "table", "dine" },
                new[] { "door", "open" },
                new[] { "window", "glass" },
                new[] { "pen", "ink" },
                new[] { "hat", "head" },
                new[] { "cake", "birthday" },
                new[] { "leaf", "green" },
                new[] { "road", "drive" },
                new[] { "train", "station" },
                new[] { "ship", "sail" },
                new[] { "shoe", "lace" },
                new[] { "cloud", "sky" },
                new[] { "cheese", "pizza" },
                new[] { "flower", "petal" },
                new[] { "cake", "icing" },
                new[] { "apple", "fruit" },
                new[] { "car", "engine" },
                new[] { "dog", "tail" }
            };

            var rnd = new Random();
            var set = wordSets[rnd.Next(wordSets.Length)];

            var game = new ImposterGame
            {
                CommonWord = set[0],
                ImposterWord = set[1],
                CurrentClueTurnIndex = 0,
                CurrentVoteTurnIndex = 0,
                CluePhaseComplete = false,
                VotePhaseComplete = false
            };

            foreach (var name in req.PlayerNames)
            {
                game.Players.Add(new ImposterPlayer
                {
                    Name = name,
                    IsImposter = false,
                    Clue = null
                });
            }

            games[game.GameId] = game;
            return Ok(new { game.GameId });
        }

        [HttpPost("join")]
        public IActionResult JoinGame([FromBody] JoinGameDTO req)
        {
            Console.WriteLine($"JoinGame called with GameId: {req.GameId}");
            if (!games.TryGetValue(req.GameId, out var game))
                return NotFound("Game not found.");

            var player = new ImposterPlayer { Name = req.Name };
            game.Players.Add(player);
            return Ok(new { player.PlayerId });
        }

        [HttpPost("start")]
        public IActionResult StartGame([FromQuery] Guid gameId)
        {
            if (!games.TryGetValue(gameId, out var game))
                return NotFound("Game not found.");

            if (game.Players.Count < 3)
                return BadRequest("At least 3 players required.");

            // RESET STATE (VERY IMPORTANT)
            foreach (var p in game.Players)
            {
                p.IsImposter = false;
                p.Clue = null;
            }

            game.CurrentClueTurnIndex = 0;
            game.CurrentVoteTurnIndex = 0;
            game.CluePhaseComplete = false;
            game.VotePhaseComplete = false;
            game.Votes.Clear();

            var rnd = new Random();
            int imposterIndex = rnd.Next(game.Players.Count);

            game.Players[imposterIndex].IsImposter = true;
            game.ImposterId = game.Players[imposterIndex].PlayerId;
            game.IsStarted = true;

            return Ok();
        }

        [HttpGet("word")]
        public IActionResult GetWord(Guid gameId, Guid playerId)
        {
            if (!games.TryGetValue(gameId, out var game))
                return NotFound("Game not found");

            var player = game.Players.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
                return NotFound("Player not found");

            string wordToShow;
            if (player.IsImposter)
            {
                // Find the hint for the current word
                var hint = game.ImposterWord;
                wordToShow = hint;
            }
            else
            {
                wordToShow = game.CommonWord;
            }
            return Ok(new
            {
                word = wordToShow,
                isImposter = player.IsImposter
            });
        }

        [HttpPost("submit-clue")]
        public IActionResult SubmitClue([FromBody] ClueRequestDTO req)
        {
            if (!games.TryGetValue(req.GameId, out var game))
                return NotFound("Game not found");

            if (game.CluePhaseComplete)
                return BadRequest("Clue phase already completed");

            var currentPlayer = game.Players[game.CurrentClueTurnIndex];

            if (currentPlayer.PlayerId != req.PlayerId)
                return BadRequest("Not your turn");

            currentPlayer.Clue = req.Clue;

            game.CurrentClueTurnIndex++;

            if (game.CurrentClueTurnIndex >= game.Players.Count)
            {
                game.CluePhaseComplete = true;
                game.CurrentVoteTurnIndex = 0;
            }

            return Ok(new
            {
                cluePhaseComplete = game.CluePhaseComplete
            });
        }

        [HttpPost("vote")]
        public IActionResult Vote([FromBody] VoteRequestDTO req)
        {
            if (!games.TryGetValue(req.GameId, out var game))
                return NotFound();

            game.Votes[req.VoterId] = req.SuspectId;
            // Advance vote turn
            if (game.CurrentVoteTurnIndex < game.Players.Count - 1)
            {
                game.CurrentVoteTurnIndex++;
            }
            else
            {
                game.VotePhaseComplete = true;
            }
            // Check if all players have voted
            if (game.Votes.Count == game.Players.Count)
            {
                var votedOut = game.Votes.Values
                    .GroupBy(x => x)
                    .OrderByDescending(g => g.Count())
                    .First().Key;

                game.IsFinished = true;
                var isImposterCaught = votedOut == game.ImposterId;
                return Ok(new { finished = true, imposterCaught = isImposterCaught });
            }

            return Ok(new { finished = false });
        }
        [HttpGet("result")]
        public IActionResult GetResult([FromQuery] Guid gameId)
        {
            if (!games.TryGetValue(gameId, out var game))
                return NotFound();

            if (!game.IsFinished)
                return BadRequest("Game not finished yet");

            var imposter = game.Players.FirstOrDefault(p => p.IsImposter);
            var votedOut = game.Votes.Values
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            return Ok(new
            {
                imposterId = imposter?.PlayerId,
                imposterName = imposter?.Name,
                votedOut = votedOut,
                isImposterCaught = votedOut == game.ImposterId,
                votes = game.Votes
            });
        }
    }
}
