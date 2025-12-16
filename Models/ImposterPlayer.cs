using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Models
{
    public class ImposterPlayer
    {
        public Guid PlayerId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool IsImposter { get; set; }
        public string Clue { get; set; }
    }
}