using System.ComponentModel.DataAnnotations;
namespace Domain.Entity
{
  

     
        public class IdeaReview
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [MaxLength(500)]
            public string Comment { get; set; } = string.Empty;

            public int? Rating { get; set; } // 1-5

            public Guid IdeaId { get; set; }
            public virtual Idea Idea { get; set; } = null!;

            public Guid ReviewerId { get; set; }
            public virtual User Reviewer { get; set; } = null!;

            public ReviewType Type { get; set; } = ReviewType.General;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        public enum ReviewType
        {
            General,
            Technical,
            Business,
            Investment
        }
    }
 
