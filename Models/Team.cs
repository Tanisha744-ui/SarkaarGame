using System.ComponentModel.DataAnnotations;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Sarkaar_Apis.Models
{
    public class Team
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? TeamLeadUsername { get; set; }
        [Required]
        public string GameCode { get; set; }
        [Required]
        public decimal Balance { get; set; }

        public ICollection<Bid> Bids { get; set; }
    }
}