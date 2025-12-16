using System.ComponentModel.DataAnnotations;

public class Team
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public User TeamLead { get; set; }
    public string TeamLeadUsername { get; set; }
    [Required]
    public int TeamLeadId { get; set; }
}