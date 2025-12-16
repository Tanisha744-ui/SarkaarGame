using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Models
{
    public class ImposterGame
    {
        public Guid GameId { get; set; } = Guid.NewGuid();
        public List<ImposterPlayer> Players { get; set; } = new();
        public string CommonWord { get; set; }
        public string ImposterWord { get; set; }
        public Guid ImposterId { get; set; }
        public bool IsStarted { get; set; }
        public bool IsFinished { get; set; }
        public Dictionary<Guid, Guid> Votes { get; set; } = new();
        public int CurrentClueTurnIndex { get; set; } = 0;
        public int CurrentVoteTurnIndex { get; set; } = 0;
        public bool CluePhaseComplete { get; set; } = false;
        public bool VotePhaseComplete { get; set; } = false;
    }
}