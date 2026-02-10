namespace Sarkaar_Apis.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string? Name { get; set; }
        public int PartyId { get; set; }
        public Party? Party { get; set; }
    }
}