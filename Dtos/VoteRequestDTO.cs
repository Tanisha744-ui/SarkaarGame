using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Dtos
{
    public class VoteRequestDTO
    {
        public Guid GameId { get; set; }
        public Guid VoterId { get; set; }
        public Guid SuspectId { get; set; }
    }
}