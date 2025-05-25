using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telerdyoloji_Backend.Domain.Entities;
[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("login_id")]
    public Guid LoginId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("first_name")]
    public string FirstName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    [Column("last_name")]
    public string LastName { get; set; } = default!;
    public string FullName => string.Join(" ", FirstName, LastName);

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = default!;

    [MaxLength(20)]
    [Column("phone")]
    public string Phone { get; set; } = default!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [MaxLength(50)]
    [Column("user_code")]
    public string UserCode { get; set; } = default!;
    [Column("is_email_verified")]
    public bool IsEmailVerified { get; set; }

    [Column("lockout_enabled")]
    public bool LockoutEnabled { get; set; }

    [Column("lockout_end")]
    public DateTime? LockoutEnd { get; set; }
    [Column("failed_login_attempts")]
    public int FailedLoginAttempts { get; set; } = 0;
    // Navigation Properties
    [ForeignKey("LoginId")]
    public virtual Login Login { get; set; } = default!;
}