namespace Sarkaar_Apis.DTOs
{
    public class PartyDetailsDto
    {
        public string PartyCode { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public List<string> Players { get; set; } = new List<string>();
    }
}