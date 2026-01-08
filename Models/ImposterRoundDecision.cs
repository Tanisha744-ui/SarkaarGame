using System;

namespace Sarkaar_Apis.Models
{
    public class ImposterRoundDecision
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public string Decision { get; set; } = null!;
        public int Round { get; set; }
    }
}
