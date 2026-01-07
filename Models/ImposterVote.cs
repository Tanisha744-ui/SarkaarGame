using System;
using System.ComponentModel.DataAnnotations.Schema;

public class ImposterVote
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid VoterId { get; set; }
    public Guid SuspectId { get; set; }

    [ForeignKey(nameof(GameId))]
    public virtual ImposterGame ImposterGame { get; set; }
}
