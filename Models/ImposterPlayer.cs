using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Sarkaar_Apis.Models
{
    public class ImposterPlayer
    {
        [Key]
        public Guid PlayerId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Clue { get; set; }
        public bool IsImposter { get; set; }
        public Guid GameId { get; set; }
        public ImposterGame Game { get; set; }

        // Add other properties as needed
    }
}