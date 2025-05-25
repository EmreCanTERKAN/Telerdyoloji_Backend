using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerdyoloji_Backend.Domain.Entities;

[Table("roles")]
public class Role
{
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty; // Initialize to avoid nullability issue  

    [MaxLength(100)]
    [Column("description")]
    public string Description { get; set; } = string.Empty; // Initialize to avoid nullability issue  

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Navigation Properties  
    public virtual ICollection<Login> Logins { get; set; }

    public Role()
    {
        Logins = new HashSet<Login>();
    }
}