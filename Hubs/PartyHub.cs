using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Sarkaar_Apis.Hubs
{
    public class PartyHub : Hub
    {
        // Store game started state in memory (for demo; use persistent store for production)
        private static readonly Dictionary<string, bool> PartyStartedStates = new Dictionary<string, bool>();
        // Store party lock state in memory (for demo; use persistent store for production)
        private static readonly Dictionary<string, bool> PartyLockStates = new Dictionary<string, bool>();

        // Helper for PartyController to check lock and started state
        public static void GetPartyLockAndStartedState(string partyCode, out bool isLocked, out bool isStarted)
        {
            isLocked = false;
            isStarted = false;
            if (PartyLockStates.TryGetValue(partyCode, out var locked))
                isLocked = locked;
            if (PartyStartedStates.TryGetValue(partyCode, out var started))
                isStarted = started;
        }

        public async Task SetPartyLockState(string partyCode, bool isLocked)
        {
            PartyLockStates[partyCode] = isLocked;
            await Clients.Group(partyCode).SendAsync("PartyLockStateChanged", isLocked);
        }

        // Store player lists in memory (for demo; use persistent store for production)
        private static readonly Dictionary<string, HashSet<string>> PartyPlayers = new Dictionary<string, HashSet<string>>();

        public async Task JoinParty(string partyCode, string playerName)
        {
            // Check if party is locked
            if (PartyLockStates.TryGetValue(partyCode, out bool isLocked) && isLocked)
            {
                await Clients.Caller.SendAsync("PartyJoinRejected", "Party is locked. You cannot join at this time.");
                return;
            }
            // Check if game has started
            if (PartyStartedStates.TryGetValue(partyCode, out bool isStarted) && isStarted)
            {
                await Clients.Caller.SendAsync("PartyJoinRejected", "Game has already started. Please wait for the next game!");
                return;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, partyCode);

            // Add player to party list
            lock (PartyPlayers)
            {
                if (!PartyPlayers.ContainsKey(partyCode))
                    PartyPlayers[partyCode] = new HashSet<string>();
                PartyPlayers[partyCode].Add(playerName);
            }

            // Broadcast updated player list to all
            List<string> players;
            lock (PartyPlayers)
            {
                players = PartyPlayers[partyCode].ToList();
            }
            await Clients.Group(partyCode).SendAsync("ReceivePlayerList", players);
            // Also send the list directly to the newly joined/refreshed client
            await Clients.Caller.SendAsync("ReceivePlayerList", players);
        }

        // Host sends called numbers, all players receive
        public async Task SendCalledNumbers(string partyCode, int[] numbers)
        {
            await Clients.Group(partyCode).SendAsync("ReceiveCalledNumbers", numbers);
        }

        public async Task SendPlayerList(string partyCode, List<string> players)
        {
            await Clients.Group(partyCode).SendAsync("ReceivePlayerList", players);
        }
        // Game started event for all players
        public async Task SendGameStarted(string partyCode)
        {
            PartyStartedStates[partyCode] = true;
            await Clients.Group(partyCode).SendAsync("ReceiveGameStarted");
        }
        // Optionally, add a method to reset the started state (e.g., for restarting the game)
        public async Task ResetGameStarted(string partyCode)
        {
            PartyStartedStates[partyCode] = false;
            await Clients.Group(partyCode).SendAsync("ReceiveGameRestarted");
        }

        // End game for all clients in the party
        public async Task SendEndGame(string partyCode)
        {
            await Clients.Group(partyCode).SendAsync("ReceiveEndGame");
        }

        // Store claimed bonuses per party: { partyCode: { bonusType: playerName } }
        private static readonly Dictionary<string, Dictionary<string, string>> Claimedbonuses = new Dictionary<string, Dictionary<string, string>>();

        public async Task ClaimbonusCard(string partyCode, string bonusType, string playerName)
        {
            lock (Claimedbonuses)
            {
                if (!Claimedbonuses.ContainsKey(partyCode))
                    Claimedbonuses[partyCode] = new Dictionary<string, string>();
                // If already claimed, do nothing
                if (Claimedbonuses[partyCode].ContainsKey(bonusType))
                {
                    // Optionally notify only the caller that it's already claimed
                    Clients.Caller.SendAsync("bonusCardAlreadyClaimed", bonusType, Claimedbonuses[partyCode][bonusType]);
                    return;
                }
                Claimedbonuses[partyCode][bonusType] = playerName;
            }
            await Clients.Group(partyCode).SendAsync("bonusCardClaimed", bonusType, playerName);
        }
        // Broadcast status updates to all clients in the party
        public async Task SendStatusUpdate(string partyCode, string status)
        {
            await Clients.Group(partyCode).SendAsync("ReceiveStatusUpdate", status);
        }
        public async Task LeaveParty(string partyCode, string playerName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, partyCode);
            lock (PartyPlayers)
            {
                if (PartyPlayers.ContainsKey(partyCode))
                {
                    PartyPlayers[partyCode].Remove(playerName);
                }
            }
            List<string> players;
            lock (PartyPlayers)
            {
                players = PartyPlayers.ContainsKey(partyCode) ? PartyPlayers[partyCode].ToList() : new List<string>();
            }
            await Clients.Group(partyCode).SendAsync("ReceivePlayerList", players);
        }
        // Broadcast Full House claimed to all clients in the party
        public async Task SendFullHouseClaimed(string partyCode)
        {
            await Clients.Group(partyCode).SendAsync("ReceiveFullHouseClaimed");
        }
    }
}
