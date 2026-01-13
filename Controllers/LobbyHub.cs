using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Sarkaar_Apis.Models;
using Microsoft.EntityFrameworkCore;
using Sarkaar_Apis.Models;
using Microsoft.AspNetCore.Mvc;


namespace backend.Controllers
{

    public class LobbyHub : Hub
    {
        private readonly SarkaarDbContext _db;
        // Store lobbies in-memory for now (replace with DB for production)
        // private static Dictionary<string, Lobby> lobbies = new();
        private static Dictionary<string, Guid> Connections = new();

        private static Dictionary<string, int> LobbyMaxPlayers = new(); // lobbyCode -> maxPlayers

        // private static Dictionary<string, Dictionary<string, string>> lobbyClues = new();
        // private static Dictionary<string, Dictionary<string, string>> proceedVotes = new();
        // private static Dictionary<string, Dictionary<string, string>> lobbyVotes = new();

        public LobbyHub(SarkaarDbContext db)
        {
            _db = db;
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Connections.ContainsKey(Context.ConnectionId))
                Connections.Remove(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateLobby(string playerName, int maxPlayers)
        {
            try
            {
                // Generate a unique lobby code (e.g., 6 uppercase letters)
                var code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

                // Create game in DB
                var game = new ImposterGame
                {
                    LobbyCode = code,
                    CreatedAt = DateTime.UtcNow,
                    IsStarted = false,
                    IsFinished = false,
                    CommonWord = "",
                    ImposterWord = "",
                    Step = "",
                    Result = ""
                };
                _db.ImposterGames.Add(game);
                await _db.SaveChangesAsync();

                // Add host as first player
                var player = new ImposterPlayer
                {
                    Name = playerName,
                    GameId = game.Id
                };
                _db.ImposterPlayers.Add(player);
                await _db.SaveChangesAsync();

                // Track max players for this lobby
                LobbyMaxPlayers[code] = maxPlayers;

                Connections[Context.ConnectionId] = player.PlayerId;

                await Groups.AddToGroupAsync(Context.ConnectionId, code);

                await Clients.Caller.SendAsync("LobbyCreated", code);
                await Clients.Group(code).SendAsync("PlayerJoined", playerName);

                // If only one player, don't start yet
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("LobbyError", ex.Message);
                throw; // This will still show in the console, but now you'llget the real error message
            }
        }


        public async Task JoinLobby(string lobbyCode, string playerName)
        {
            try
            {
                var game = await _db.ImposterGames
                    .Include(g => g.Players)
                    .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode && !g.IsFinished);

                if (game == null)
                {
                    await Clients.Caller.SendAsync("LobbyError", "Lobby not found.");
                    return;
                }

                if (game.Players == null)
                {
                    await Clients.Caller.SendAsync("LobbyError", "Lobby has no players.");
                    return;
                }

                if (game.Players.Any(p => (p.Name ?? "") == (playerName ?? "")))
                {
                    await Clients.Caller.SendAsync("LobbyError", "Name already taken.");
                    return;
                }

                var player = new ImposterPlayer
                {
                    Name = playerName ?? "",
                    Clue = "",
                    GameId = game.Id
                };

                _db.ImposterPlayers.Add(player);
                await _db.SaveChangesAsync();

                Connections[Context.ConnectionId] = player.PlayerId;
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyCode);

                // Reload game to get updated players
                var updatedGame = await _db.ImposterGames
                    .Include(g => g.Players)
                    .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode && !g.IsFinished);

                if (updatedGame == null)
                {
                    await Clients.Caller.SendAsync("LobbyError", "Lobby not found after join.");
                    return;
                }

                if (updatedGame.Players == null)
                {
                    await Clients.Caller.SendAsync("LobbyError", "Lobby players missing after join.");
                    return;
                }

                // Send only player names as string[]
                var allPlayers = updatedGame.Players.Select(p => p.Name ?? "").ToList();
                await Clients.Group(lobbyCode).SendAsync("PlayerJoined", playerName ?? "");

                if (LobbyMaxPlayers.TryGetValue(lobbyCode, out var maxPlayers) && allPlayers.Count >= maxPlayers)
                {
                    updatedGame.IsStarted = true;
                    await _db.SaveChangesAsync();

                    await Clients.Group(lobbyCode).SendAsync("AllPlayersJoined", allPlayers);

                    await AssignWords(lobbyCode);
                }
                // ADD THIS: update viewers after player joins
                await SendViewerUpdate(lobbyCode);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("LobbyError", ex.Message);
            }
        }

        private string GenerateLobbyCode()
        {
            // Simple random code generator
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

        public async Task AssignWords(string code)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == code);

            if (game == null) return;

            // Prevent re-assignment if words are already set
            if (!string.IsNullOrEmpty(game.CommonWord) && !string.IsNullOrEmpty(game.ImposterWord))
                return;

            var rnd = new Random();
            var playersList = game.Players.ToList();

            // 🔴 Set all to NOT imposter first
            foreach (var p in playersList)
                p.IsImposter = false;

            var imposter = playersList[rnd.Next(playersList.Count)];
            game.ImposterId = imposter.PlayerId;
            imposter.IsImposter = true;

            var wordSets = new[]
            {
                new[] { "Airport", "Boarding" },
                new[] { "Hospital", "Medicine" },
                new[] { "School", "Homework" },
                new[] { "Restaurant", "Menu" },
                new[] { "Cinema", "Trailer" },
                new[] { "Hotel", "Keycard" },
                new[] { "Gym", "Weights" },
                new[] { "Library", "Study" },
                new[] { "Office", "Meeting" },
                new[] { "Station", "Platform" },
                new[] { "Pizza", "Cheese" },
                new[] { "Burger", "Sauce" },
                new[] { "Coffee", "Caffeine" },
                new[] { "Ice Cream", "Cone" },
                new[] { "Sandwich", "Toast" },
                new[] { "Cake", "Slice" },
                new[] { "Pasta", "Boil" },
                new[] { "Soup", "Steam" },
                new[] { "Chocolate", "Bitter" },
                new[] { "Bread", "Fresh" },
                new[] { "Phone", "Battery" },
                new[] { "Laptop", "Charger" },
                new[] { "Camera", "Zoom" },
                new[] { "Car", "Fuel" },
                new[] { "Watch", "Alarm" },
                new[] { "Shoes", "Comfort" },
                new[] { "Bag", "Books" },
                new[] { "Pen", "Signature" },
                new[] { "Bottle", "Water" },
                new[] { "Headphones", "Volume" },
                new[] { "Rain", "Umbrella" },
                new[] { "Sun", "Heat" },
                new[] { "Snow", "Cold" },
                new[] { "River", "Flow" },
                new[] { "Forest", "Trees" },
                new[] { "Beach", "Sand" },
                new[] { "Fire", "Smoke" },
                new[] { "Wind", "Breeze" },
                new[] { "Night", "Dark" },
                new[] { "Morning", "Fresh" },
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
            var words = wordSets[rnd.Next(wordSets.Length)];

            game.CommonWord = words[0];
            game.ImposterWord = words[1];

            // Save changes so IsImposter and words are persisted
            await _db.SaveChangesAsync();

            // 🔴 Reload players to get updated IsImposter values
            game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == code);

            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var p in game.Players)
            {
                var connectionId = Connections.FirstOrDefault(x => x.Value == p.PlayerId).Key;
                if (connectionId == null) continue;

                await Clients.Client(connectionId).SendAsync("WordAssigned", new
                {
                    word = p.IsImposter ? game.ImposterWord : game.CommonWord,
                    isImposter = p.IsImposter,
                    wordStartTime = startTime
                });
            }
        }

        public async Task SelectMode(string code, string mode)
        {
            await Clients.Group(code).SendAsync("ModeSelected", mode);
        }
        public async Task RevealImposter(string lobbyCode)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null) return;

            var imposter = game.Players.FirstOrDefault(p => p.PlayerId == game.ImposterId);

            await Clients.Group(lobbyCode)
                .SendAsync("ImposterRevealed", imposter?.Name);

            // --- Cleanup game data after revealing the imposter ---
            var gameId = game.Id;

            // Remove clues
            var clues = _db.Set<ImposterClue>().Where(c => c.GameId == gameId);
            _db.Set<ImposterClue>().RemoveRange(clues);

            // Remove votes
            var votes = _db.Set<ImposterVote>().Where(v => v.GameId == gameId);
            _db.Set<ImposterVote>().RemoveRange(votes);

            // Remove round decisions
            var decisions = _db.Set<Sarkaar_Apis.Models.ImposterRoundDecision>().Where(d => d.GameId == gameId);
            _db.Set<Sarkaar_Apis.Models.ImposterRoundDecision>().RemoveRange(decisions);

            // Remove players
            var players = _db.Set<ImposterPlayer>().Where(p => p.GameId == gameId);
            _db.Set<ImposterPlayer>().RemoveRange(players);

            // Remove the game itself
            _db.Set<ImposterGame>().Remove(game);

            await _db.SaveChangesAsync();
        }


        public async Task RequestWord(string code)
        {
            await AssignWords(code);
        }
        public async Task SendWord(string lobbyCode)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null) return;

            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var player in game.Players)
            {
                var connectionId = Connections.FirstOrDefault(x => x.Value == player.PlayerId).Key;

                if (connectionId == null) continue;

                await Clients.Client(connectionId).SendAsync("WordAssigned", new
                {
                    word = player.IsImposter ? "You are the Imposter" : game.CommonWord,
                    isImposter = player.IsImposter,
                    wordStartTime = startTime
                });
            }
        }


        public async Task SubmitClue(string lobbyCode, string playerName, string clue)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null) return;

            var player = game.Players.FirstOrDefault(p => p.Name == playerName);
            if (player == null) return;

            // Save clue in DB
            var imposterClue = new ImposterClue
            {
                GameId = game.Id,
                PlayerId = player.PlayerId,
                Round = game.Round,
                Clue = clue
            };
            _db.ImposterClues.Add(imposterClue);
            await _db.SaveChangesAsync(); // <--- THIS IS CRUCIAL

            // After saving the clue, broadcast to viewers
            await Clients.Group(lobbyCode + "_viewers").SendAsync("ViewerClueUpdate", new {
                player = playerName,
                clue = clue
            });

            // Check if all players have submitted clues for this round
            var cluesThisRound = await _db.ImposterClues
                .Where(c => c.GameId == game.Id && c.Round == game.Round)
                .ToListAsync();

            if (cluesThisRound.Count == game.Players.Count)
            {
                // Prepare clues dictionary
                var cluesDict = game.Players.ToDictionary(
                    p => p.Name ?? "",
                    p => cluesThisRound.FirstOrDefault(c => c.PlayerId == p.PlayerId)?.Clue ?? ""
                );

                // Broadcast to all clients
                await Clients.Group(lobbyCode).SendAsync("AllCluesSubmitted", cluesDict);
            }
        }

        public async Task ProceedOrNextRound(string lobbyCode, string playerName, string action)
        {
            try
            {
                var game = await _db.ImposterGames
                    .Include(g => g.Players)
                    .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

                if (game == null) return;

                var player = game.Players.FirstOrDefault(p => p.Name == playerName);
                if (player == null)
                {
                    await Clients.Caller.SendAsync("LobbyError", "Player not found in this game.");
                    return;
                }

                var existing = await _db.ImposterRoundDecisions
                    .FirstOrDefaultAsync(x => x.GameId == game.Id && x.PlayerId == player.PlayerId);

                // Find the current round
                var round = game.Round;

                if (existing == null)
                {
                    _db.ImposterRoundDecisions.Add(new ImposterRoundDecision
                    {
                        GameId = game.Id,
                        PlayerId = player.PlayerId,
                        Decision = action,
                        Round = game.Round // <-- Make sure this property exists and is set
                    });
                    await _db.SaveChangesAsync();
                    await SendViewerUpdate(lobbyCode); // <-- Always update viewers
                }
                else
                {
                    existing.Decision = action;
                }

                await _db.SaveChangesAsync();

                var decisions = await _db.ImposterRoundDecisions
                    .Where(x => x.GameId == game.Id)
                    .ToListAsync();

                // ANYONE wants next round
                if (decisions.Any(d => d.Decision == "next"))
                {
                    _db.ImposterRoundDecisions.RemoveRange(decisions);

                    // Increment round number for the game
                    game.Round += 1;
                    await _db.SaveChangesAsync();

                    await Clients.Group(lobbyCode).SendAsync("StartNextRound");
                    return;
                }

                // ALL want voting
                if (decisions.Count == game.Players.Count &&
                    decisions.All(d => d.Decision == "vote"))
                {
                    _db.ImposterRoundDecisions.RemoveRange(decisions);
                    await _db.SaveChangesAsync();

                    await Clients.Group(lobbyCode).SendAsync("ProceedToVoting");
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                await Clients.Caller.SendAsync("LobbyError", errorMsg);
                throw;
            }
        }


        public async Task VoteFor(string lobbyCode, string voterName, string suspectName)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null) return;

            var voter = game.Players.FirstOrDefault(p => p.Name == voterName);
            var suspect = game.Players.FirstOrDefault(p => p.Name == suspectName);

            if (voter == null || suspect == null)
            {
                await Clients.Caller.SendAsync("LobbyError", "Invalid voter or suspect.");
                return;
            }

            // Check if this voter already voted
            var existingVote = await _db.Set<ImposterVote>()
                .FirstOrDefaultAsync(v => v.GameId == game.Id && v.VoterId == voter.PlayerId);

            if (existingVote == null)
            {
                _db.Add(new ImposterVote
                {
                    GameId = game.Id,
                    VoterId = voter.PlayerId,
                    SuspectId = suspect.PlayerId
                });
            }
            else
            {
                existingVote.SuspectId = suspect.PlayerId;
            }
            await _db.SaveChangesAsync();

            // Count votes
            var votes = await _db.Set<ImposterVote>()
                .Where(v => v.GameId == game.Id)
                .ToListAsync();

            if (votes.Count == game.Players.Count)
            {
                // Tally votes
                var grouped = votes.GroupBy(v => v.SuspectId)
                    .Select(g => new { SuspectId = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .ToList();

                var imposter = game.Players.FirstOrDefault(p => p.PlayerId == game.ImposterId);

                // Find how many votes the imposter got
                int imposterVotes = grouped.FirstOrDefault(g => g.SuspectId == imposter.PlayerId)?.Count ?? 0;

                // More than 50% required
                int totalPlayers = game.Players.Count;
                bool imposterCaught = imposterVotes > totalPlayers / 2;

                // Find the accused (the player with the most votes)
                var topGroup = grouped.FirstOrDefault();
                var accused = topGroup != null ? game.Players.FirstOrDefault(p => p.PlayerId == topGroup.SuspectId) : null;

                game.Result = $"{imposterCaught}|{accused?.Name}|{imposter?.Name}";
                game.Step = "result";
                await _db.SaveChangesAsync();

                await Clients.Group(lobbyCode).SendAsync("VotingResult", imposterCaught, accused?.Name, imposter?.Name);
                await SendViewerUpdate(lobbyCode);
            }
            else
            {
                await SendViewerUpdate(lobbyCode); // <-- Add this
            }
        }

        public async Task SeeWordAgain(string lobbyCode)
        {
            await Clients.Group(lobbyCode).SendAsync("SeeWordAgain");
        }

        public async Task Cleanup(string lobbyCode)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .Include(g => g.Clues)
                .Include(g => g.Votes)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null)
            {
                await Clients.Caller.SendAsync("LobbyError", "Game not found for cleanup.");
                return;
            }

            _db.ImposterPlayers.RemoveRange(game.Players);
            _db.ImposterClues.RemoveRange(game.Clues);
            _db.ImposterVotes.RemoveRange(game.Votes);

            // Remove round decisions if you have them
            var decisions = _db.ImposterRoundDecisions.Where(d => d.GameId == game.Id);
            _db.ImposterRoundDecisions.RemoveRange(decisions);

            // _db.ImposterRounds.RemoveRange(game.Rounds); // REMOVE THIS LINE

            _db.ImposterGames.Remove(game);

            await _db.SaveChangesAsync();

            await Clients.Caller.SendAsync("CleanupSuccess", lobbyCode);
        }

        public async Task JoinAsViewer(string lobbyCode)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, lobbyCode + "_viewers");
                await SendViewerUpdate(lobbyCode);
            }
            catch (Exception ex)
            {
                // Log ex.Message and ex.StackTrace
                await Clients.Caller.SendAsync("LobbyError", ex.Message);
            }
        }

        private async Task SendViewerUpdate(string lobbyCode)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);

            if (game == null) return;

            var clues = await _db.Set<ImposterClue>()
                .Where(c => c.GameId == game.Id)
                .ToListAsync();

            var decisions = await _db.Set<ImposterRoundDecision>()
                .Where(d => d.GameId == game.Id)
                .ToListAsync();

            var votes = await _db.Set<ImposterVote>()
                .Where(v => v.GameId == game.Id)
                .ToListAsync();

            var rounds = clues
                .GroupBy(c => c.Round)
                .OrderBy(g => g.Key)
                .Select(g => new {
                    round = g.Key,
                    clues = g.Select(c => new {
                        PlayerId = c.PlayerId,
                        Name = game.Players.FirstOrDefault(p => p.PlayerId == c.PlayerId)?.Name ?? "",
                        text = c.Clue ?? ""
                    }).ToList(),
                    decisions = decisions
                        .Where(d => d.Round == g.Key)
                        .Select(d => new {
                            PlayerId = d.PlayerId,
                            Name = game.Players.FirstOrDefault(p => p.PlayerId == d.PlayerId)?.Name ?? "",
                            action = d.Decision ?? ""
                        }).ToList()
                }).ToList();

            var votesList = votes.Select(v => new {
                VoterId = v.VoterId,
                SuspectId = v.SuspectId,
                voterName = game.Players.FirstOrDefault(p => p.PlayerId == v.VoterId)?.Name ?? "",
                suspectName = game.Players.FirstOrDefault(p => p.PlayerId == v.SuspectId)?.Name ?? ""
            }).ToList();

            object result = null;
            if (game.Step == "result" && !string.IsNullOrEmpty(game.Result))
            {
                result = game.Result;
            }

            var data = new {
                players = game.Players.Select(p => new {
                    PlayerId = p.PlayerId,
                    Name = p.Name ?? "",
                    IsImposter = p.PlayerId == game.ImposterId,
                    Word = p.IsImposter ? (game.ImposterWord ?? "") : (game.CommonWord ?? "")
                }).ToList(),
                rounds,
                votes = votesList,
                step = game.Step ?? "",
                result
            };

            await Clients.Group(lobbyCode + "_viewers").SendAsync("ViewerUpdate", data);
        }

        [HttpGet("viewer-state")]
        public async Task<IActionResult> GetViewerState([FromQuery] Guid gameId)
        {
            var game = await _db.ImposterGames
                .Include(g => g.Players)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
                return new NotFoundResult();

            // Get clues and votes from DB
            var clues = await _db.Set<ImposterClue>()
                .Where(c => c.GameId == gameId)
                .ToListAsync();

            var votes = await _db.Set<ImposterVote>()
                .Where(v => v.GameId == gameId)
                .ToListAsync();

            return new OkObjectResult(new
            {
                players = game.Players.Select(p => new {
                    playerId = p.PlayerId,
                    name = p.Name,
                    isImposter = p.IsImposter
                }),
                clues = clues.Select(c => new {
                    playerId = c.PlayerId,
                    clue = c.Clue
                }),
                votes = votes.Select(v => new {
                    voterId = v.VoterId,
                    suspectId = v.SuspectId
                }),
                commonWord = game.CommonWord,
                imposterWord = game.ImposterWord,
                imposterId = game.ImposterId,
                isFinished = game.IsFinished
            });
        }
    }
}