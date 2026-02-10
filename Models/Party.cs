namespace Sarkaar_Apis.Models
{
    public class Party
    {
        public int PartyId { get; set; }
        public string? PartyCode { get; set; } // Unique code for the party
        public string? HostName { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
    }
}