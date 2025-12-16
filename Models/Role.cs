using System.ComponentModel.DataAnnotations;

public class Role
{
    [Key]
    public int RoleId { get; set; }
    [Required]
    public string Name { get; set; }
}