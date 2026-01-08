using System.ComponentModel.DataAnnotations;

namespace SarkaarGame.Models
{
    public class GameControls
    {
        [Key]
        public int Id { get; set; }
        public string GameCode { get; set; }
        public int Interval { get; set; }
        public int MaxBidAmount { get; set; }
    }
}