using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sarkaar_Apis.Models
{
    public class ImposterPlayer
    {
        [Key]
        public Guid PlayerId { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Clue { get; set; }
        public bool IsImposter { get; set; }
        public Guid GameId { get; set; }
        public ImposterGame Game { get; set; }

        // Add other properties as needed
    }
}