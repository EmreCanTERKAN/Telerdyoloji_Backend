using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerdyoloji_Backend.Domain.Entities;
[Table("login")]
public class Login
{
    [Key]
    [Column("login_id")]
    public Guid LoginId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = default!;

    [Required]
    [MaxLength(255)]
    [Column("password")]
    public string Password { get; set; } = default!;

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [MaxLength(50)]
    [Column("user_code")]
    public string UserCode { get; set; } = default!;

    [MaxLength(255)]
    [Column("refresh_token")]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expires")]
    public DateTime? RefreshTokenExpires { get; set; }

    // Navigation Properties
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; } = default!;

    public virtual User User { get; set; } = default!;
}
