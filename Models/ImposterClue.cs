using System;
using System.ComponentModel.DataAnnotations.Schema;

public class ImposterClue
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public int Round { get; set; }
    public string? Clue { get; set; }

    [ForeignKey(nameof(GameId))]
    public virtual ImposterGame ImposterGame { get; set; }
}