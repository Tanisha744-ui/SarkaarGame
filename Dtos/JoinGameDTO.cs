using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Dtos
{
    public class JoinGameDTO
    {
        public Guid GameId { get; set; }
        public string Name { get; set; }
    }
}