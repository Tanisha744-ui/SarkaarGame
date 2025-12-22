using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class Player
    {
        // Define properties for Player as needed
        public string Id { get; set; }
        public string Name { get; set; }
    }

    class LobbyGame
    {
        public string LobbyId { get; set; }
        public List<Player> Players { get; set; } = new();
        public Dictionary<string, string> Clues { get; set; } = new();
        public Dictionary<string, string> Votes { get; set; } = new();
        public bool CluePhaseEnded { get; set; }
    }

}