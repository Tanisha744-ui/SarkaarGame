namespace Sarkaar_Apis.Dtos
{
    public class BidDto
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int Amount { get; set; }
        public bool IsActive { get; set; }
        public int? GameId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBidDto
    {
        public int TeamId { get; set; }
        public int Amount { get; set; }
        public int? GameId { get; set; }
    }
}
