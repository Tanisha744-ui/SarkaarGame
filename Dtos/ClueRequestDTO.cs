using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Dtos
{
    public class ClueRequestDTO
    {
        
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
        public string Clue { get; set; }
    }
}