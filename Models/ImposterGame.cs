using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sarkaar_Apis.Models
{
    [Table("ImposterGames")]
    public class ImposterGame
    {
        public Guid Id { get; set; }
        public virtual List<ImposterPlayer> Players { get; set; } = new();
        public string CommonWord { get; set; }
        public string ImposterWord { get; set; }
        public Guid ImposterId { get; set; }
        public bool IsStarted { get; set; }
        public bool IsFinished { get; set; }
        // Votes will be handled separately, not as a Dictionary for EF Core
        public int CurrentClueTurnIndex { get; set; } = 0;
        public int CurrentVoteTurnIndex { get; set; } = 0;
        public bool CluePhaseComplete { get; set; } = false;
        public bool VotePhaseComplete { get; set; } = false;

        // Add this property for votes
        [NotMapped]
        public Dictionary<Guid, Guid> Votes { get; set; } = new Dictionary<Guid, Guid>();

        public string LobbyCode { get; set; }
    }
}