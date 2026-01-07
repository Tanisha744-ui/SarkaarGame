using System;
using System.Collections.Generic;
using Sarkaar_Apis.Models;

public class ImposterGame
{
    public Guid Id { get; set; }
    public string LobbyCode { get; set; }= null!;

    public string? CommonWord { get; set; }
    public string? ImposterWord { get; set; }
    public Guid? ImposterId { get; set; }

    public bool IsStarted { get; set; }
    public bool IsFinished { get; set; }

    public int CurrentClueTurnIndex { get; set; }
    public int CurrentVoteTurnIndex { get; set; }

    public bool CluePhaseComplete { get; set; }
    public bool VotePhaseComplete { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    public int Round { get; set; } = 1;

    public string Step { get; set; }
    public string Result { get; set; } 
    public virtual ICollection<ImposterClue> Clues { get; set; }
    public virtual ICollection<ImposterVote> Votes { get; set; } = new List<ImposterVote>();
    public virtual ICollection<ImposterPlayer> Players { get; set; } = new List<ImposterPlayer>();
}
