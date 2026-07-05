using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entity
{
   

 
        public class Idea
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [MaxLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;

            public string? ProblemStatement { get; set; }
            public string? SolutionDescription { get; set; }
            public string? TargetAudience { get; set; }
            public string? UniqueValue { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal? EstimatedBudget { get; set; }

            public int? EstimatedTimelineMonths { get; set; }

            // محرمانگی - فیلدهای رمزگذاری شده
            public string? EncryptedDetails { get; set; }
            public string? EncryptionKeyHint { get; set; }
            public string EncryptedTitle { get; set; }   // AES-256
            public string EncryptedDescription { get; set; }
            public string? EncryptedMarketInfo { get; set; }
            public ICollection<BlindField> BlindFields { get; set; }
            // وضعیت‌ها
            public IdeaStatus Status { get; set; } = IdeaStatus.Draft;
            public VisibilityLevel Visibility { get; set; } = VisibilityLevel.Private;

            // آمار
            public int ViewCount { get; set; } = 0;
            public int LikeCount { get; set; } = 0;
            public int ReviewCount { get; set; } = 0;

            public Guid OwnerId { get; set; }
            public virtual User Owner { get; set; } = null!;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; }
            public DateTime? PublishedAt { get; set; }

            // Navigation Properties
            public virtual ICollection<IdeaReview> Reviews { get; set; } = new List<IdeaReview>();
            public virtual ICollection<IdeaCollaborator> Collaborators { get; set; } = new List<IdeaCollaborator>();
            public virtual ICollection<IdeaTag> Tags { get; set; } = new List<IdeaTag>();
        }

        public enum IdeaStatus
        {
            Draft,        // پیش‌نویس
            PendingReview, // در انتظار بررسی
            Approved,     // تأیید شده
            InProgress,   // در حال توسعه
            Completed,    // تکمیل شده
            Rejected,     // رد شده
            Archived      // بایگانی
        }

        public enum VisibilityLevel
        {
            Private,     // فقط مالک
            Confidential, // با رمز (محرمانه)
            Public       // عمومی
        }
        public class BlindField
        {
            // فیلدهایی که فقط بعد از NDA دیده می‌شن
            public string EncryptedFieldName { get; set; }
            public string EncryptedFieldValue { get; set; }
        }
    }

