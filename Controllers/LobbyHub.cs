using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Sarkaar_Apis.Models;
using Microsoft.EntityFrameworkCore;


namespace backend.Controllers
{

    public class LobbyHub : Hub
    {
        private readonly SarkaarDbContext _db;
        // Store lobbies in-memory for now (replace with DB for production)
        private static Dictionary<string, Lobby> lobbies = new();
        // Map player name to connectionId
        private static Dictionary<string, string> playerConnections = new();
        private static Dictionary<string, Dictionary<string, string>> lobbyClues = new();
        private static Dictionary<string, Dictionary<string, string>> proceedVotes = new();
        private static Dictionary<string, Dictionary<string, string>> lobbyVotes = new();

        public LobbyHub(SarkaarDbContext db)
        {
            _db = db;
        }

        public class Lobby
        {
            public string Code { get; set; }
            public string Admin { get; set; }
            public int PlayerCount { get; set; }
            public List<string> Players { get; set; }
            public string Imposter { get; set; }
            public string Word { get; set; } // Added property
            public Guid GameId { get; set; } // Add GameId property
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove player from connection map on disconnect
            var player = playerConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (player != null)
                playerConnections.Remove(player);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateLobby(string adminName, int playerCount)
        {
            var code = GenerateLobbyCode();
            var lobby = new Lobby
            {
                Code = code,
                Admin = adminName,
                PlayerCount = playerCount,
                Players = new List<string> { adminName },
                GameId = Guid.NewGuid() // Assign a unique GameId for the lobby
            };
            lobbies[code] = lobby;
            playerConnections[adminName] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, code);
            await Clients.Caller.SendAsync("LobbyCreated", code);
        }

        public async Task JoinLobby(string code, string name)
        {
            if (lobbies.TryGetValue(code, out var lobby) && lobby.Players.Count < lobby.PlayerCount)
            {
                lobby.Players.Add(name);
                playerConnections[name] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, code);
                await Clients.Group(code).SendAsync("PlayerJoined", name, lobby.Players.Count, lobby.PlayerCount);
                if (lobby.Players.Count == lobby.PlayerCount)
                {
                    await Clients.Group(code).SendAsync("AllPlayersJoined", lobby.Players);
                }

                // Find or create the game entity for this lobby
                var game = _db.ImposterGames.FirstOrDefault(g => g.LobbyCode == code);
                if (game == null)
                {
                    game = new ImposterGame
                    {
                        LobbyCode = code,
                        // Set other properties as needed
                    };
                    _db.ImposterGames.Add(game);
                    await _db.SaveChangesAsync();
                }

                // Save player to database with correct GameId
                var playerEntity = new ImposterPlayer
                {
                    Name = name,
                    GameId = game.Id, // Set the GameId
                    // Set other properties as needed
                };
                _db.ImposterPlayers.Add(playerEntity);
                await _db.SaveChangesAsync();
            }
            else
            {
                await Clients.Caller.SendAsync("LobbyJoinFailed", "Lobby full or not found");
            }
        }

        private string GenerateLobbyCode()
        {
            // Simple random code generator
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

        public async Task AssignWords(string code)
        {
            var lobby = lobbies[code];
            var random = new Random();
            var imposterIndex = random.Next(lobby.Players.Count);

            lobby.Imposter = lobby.Players[imposterIndex];

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
            var words = wordSets[random.Next(wordSets.Length)];
            lobby.Word = words[0]; // Store the main word in the lobby

            for (int i = 0; i < lobby.Players.Count; i++)
            {
                var player = lobby.Players[i];
                var connectionId = playerConnections.ContainsKey(player) ? playerConnections[player] : null;
                if (connectionId != null)
                {
                    var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    await Clients.Client(connectionId).SendAsync(
                        "WordAssigned",
                        new
                        {

                            word = player == lobby.Imposter ? words[1] : words[0],
                            isImposter = player == lobby.Imposter,
                            wordStartTime = startTime
                        }
                    );
                }
            }
        }

        public async Task SelectMode(string code, string mode)
        {
            await Clients.Group(code).SendAsync("ModeSelected", mode);
        }
        public async Task RevealImposter(string lobbyCode)
        {
            var lobby = lobbies[lobbyCode];

            var imposterName = lobby.Imposter;

            await Clients.Group(lobbyCode)
                .SendAsync("ImposterRevealed", imposterName);
        }

        public async Task RequestWord(string code)
        {
            await AssignWords(code);
        }
        public async Task SendWord(string lobbyCode)
        {
            var lobby = lobbies[lobbyCode];

            var word = lobby.Word;
            var imposter = lobby.Imposter;

            var startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var player in lobby.Players)
            {
                if (playerConnections.TryGetValue(player, out var connectionId))
                {
                    await Clients.Client(connectionId)
                        .SendAsync("WordAssigned", new
                        {
                            word = player == imposter ? "You are the Imposter" : word,
                            isImposter = player == imposter,
                            wordStartTime = startTime
                        });
                }
            }
        }

        public async Task SubmitClue(string code, string player, string clue)
        {
            if (!lobbyClues.ContainsKey(code))
                lobbyClues[code] = new Dictionary<string, string>();

            lobbyClues[code][player] = clue;

            var lobby = lobbies[code];
            if (lobbyClues[code].Count == lobby.Players.Count)
            {
                // All clues submitted, broadcast to all
                await Clients.Group(code).SendAsync("AllCluesSubmitted", lobbyClues[code]);
                lobbyClues.Remove(code);
            }
        }

        public async Task ProceedOrNextRound(string code, string player, string action)
        {
            if (!proceedVotes.ContainsKey(code))
                proceedVotes[code] = new Dictionary<string, string>();

            proceedVotes[code][player] = action;

            var lobby = lobbies[code];

            // If any player wants next round, start next round
            if (proceedVotes[code].Values.Contains("next"))
            {
                proceedVotes.Remove(code);
                await Clients.Group(code).SendAsync("StartNextRound");
                return;
            }

            // If all players want to vote, proceed to voting
            if (proceedVotes[code].Count == lobby.Players.Count &&
                proceedVotes[code].Values.All(v => v == "vote"))
            {
                proceedVotes.Remove(code);
                await Clients.Group(code).SendAsync("ProceedToVoting");
            }
        }

        public async Task VoteFor(string code, string voter, string votedPlayer)
        {
            if (!lobbyVotes.ContainsKey(code))
                lobbyVotes[code] = new Dictionary<string, string>();

            lobbyVotes[code][voter] = votedPlayer;

            var lobby = lobbies[code];
            if (lobbyVotes[code].Count == lobby.Players.Count)
            {
                // Tally votes
                var voteCounts = lobbyVotes[code].Values.GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Find the player with the most votes
                var maxVotes = voteCounts.Values.Max();
                var mostVoted = voteCounts.Where(x => x.Value == maxVotes).Select(x => x.Key).ToList();

                // If tie, pick first (or handle as you wish)
                var accused = mostVoted[0];

                // Check if accused is the imposter
                bool imposterCaught = accused == lobby.Imposter && maxVotes > lobby.Players.Count / 2;

                await Clients.Group(code).SendAsync("VotingResult", imposterCaught, accused, lobby.Imposter);

                lobbyVotes.Remove(code);
            }
        }

    }
}