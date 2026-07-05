using System.ComponentModel.DataAnnotations;

namespace Domain.Entity
{
    
        public class User
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [MaxLength(100)]
            public string FullName { get; set; } = string.Empty;

            public string DisplayName { get; set; }

            [Required]
            [EmailAddress]
            [MaxLength(150)]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string PasswordHash { get; set; } = string.Empty;

            public string? Bio { get; set; }
            public string? AvatarUrl { get; set; }

            public UserRole Role { get; set; } = UserRole.Ideator;
            public UserStatus Status { get; set; } = UserStatus.Active;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? LastLoginAt { get; set; }

            // Navigation Properties
            public virtual ICollection<Idea> Ideas { get; set; } = new List<Idea>();
            public virtual ICollection<IdeaReview> Reviews { get; set; } = new List<IdeaReview>();
        }

        public enum UserRole
        {
            Ideator,    // ثبت‌کننده ایده  Inventor
            Developer,  // توسعه‌دهنده
            Investor,   // سرمایه‌گذار
            Admin       // مدیر
        }

        public enum UserStatus
        {
            Active,
            Suspended,
            Deleted
        }
    }
 
