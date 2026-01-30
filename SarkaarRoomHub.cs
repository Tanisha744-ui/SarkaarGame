using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SarkaarRoomHub : Hub
{
    // In-memory storage for demo; use DB in production
    private static ConcurrentDictionary<string, bool> roomStarted = new();
    private static ConcurrentDictionary<string, List<string>> rooms = new();

    // New: Room and Team code storage
    private static ConcurrentDictionary<string, string> teamCodes = new(); // teamCode -> roomCode
    private static ConcurrentDictionary<string, List<string>> teamMembers = new(); // teamCode -> member names

    public async Task<object> CreateRoomWithTeamCode(string teamName)
    {
        var gameId = new System.Random().Next(100000, 999999);
        var roomCode = gameId.ToString();
        var teamCode = new System.Random().Next(10000000, 99999999).ToString();
        rooms[roomCode] = new List<string> { teamName };
        teamCodes[teamCode] = roomCode;
        teamMembers[teamCode] = new List<string>();
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode); // roomCode == gameId.ToString()
        await Clients.Group(roomCode).SendAsync("TeamsUpdated", rooms[roomCode]);
        return new { roomCode, teamCode, gameId };
    }

    public async Task<object> JoinRoomWithTeamCode(string roomCode, string teamName)
    {
        // Block join if game has started
        if (roomStarted.ContainsKey(roomCode) && roomStarted[roomCode])
            return new { success = false, teamCode = "", error = "Game already started" };
        if (!rooms.ContainsKey(roomCode) || rooms[roomCode].Count >= 10)
            return new { success = false, teamCode = "", error = "Invalid code or room full" };
        var teamCode = new System.Random().Next(10000000, 99999999).ToString();
        rooms[roomCode].Add(teamName);
        teamCodes[teamCode] = roomCode;
        teamMembers[teamCode] = new List<string>();
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode); // roomCode == gameId.ToString()
        await Clients.Group(roomCode).SendAsync("TeamsUpdated", rooms[roomCode]);
        int gameId = int.TryParse(roomCode, out var gid) ? gid : 0;
        return new { success = true, teamCode, gameId };
    }

    public async Task<bool> JoinTeamAsMember(string teamCode, string memberName)
    {
        if (!teamMembers.ContainsKey(teamCode) || teamMembers[teamCode].Count >= 10) return false;
        teamMembers[teamCode].Add(memberName);
        // Optionally add to SignalR group for team updates
        await Groups.AddToGroupAsync(Context.ConnectionId, teamCode);
        return true;
    }

    public async Task<List<string>> GetTeams(string code)
    {
        return rooms.ContainsKey(code) ? rooms[code] : new List<string>();
    }
    public async Task StartGame(string roomCode)
    {
        // Mark as started and notify all clients in the room
        roomStarted[roomCode] = true;
        await Clients.Group(roomCode).SendAsync("GameStarted");
    }
    public async Task BroadcastBid(int gameId, int teamId, int amount)
    {
        // Send bid info to all clients in the room
        Console.WriteLine($"Broadcasting bid in game {gameId} from team {teamId} amount {amount}");
        await Clients.Group(gameId.ToString()).SendAsync("BidReceived", new { gameId, teamId, amount });
    }

    // Added: Allow Angular to call SendBid, which broadcasts the bid
    public async Task SendBid(string roomCode, int teamId, int amount)
    {
        Console.WriteLine($"Received bid for roomCode: {roomCode}, teamId: {teamId}, amount: {amount}");
        await Clients.Group(roomCode)
        .SendAsync("BidReceived", new { roomCode, teamId, amount });
    }
    public async Task JoinGameGroup(string roomCode)
    {
        Console.WriteLine($"Joining group for roomCode: {roomCode}");
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
    }
    public async Task JoinRoomAsSpectator(string roomCode, string name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
    }
    public async Task SetGameStarted(string roomCode, bool started)
    {
        // Logic to handle game start
        await Clients.Group(roomCode).SendAsync("GameStarted", started);
    }

    public async Task SendChatMessage(string roomCode, string sender, string text)
    {
        Console.WriteLine($"Chat message in room {roomCode} from {sender}: {text}");
        await Clients.Group(roomCode).SendAsync("ChatMessageReceived", new { sender, text });
    }

}
