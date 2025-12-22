using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sarkaar_Apis.Models
{
    public class PlayerInfo
        {
            public string ConnectionId { get; set; }
            public string Name { get; set; }
            public string Word { get; set; }
            public bool IsImposter { get; set; }
            public string Clue { get; set; }
            public string Vote { get; set; }
        }
}