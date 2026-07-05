using System.ComponentModel.DataAnnotations;

namespace Domain.Entity
{
   
    
        public class IdeaTag
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [MaxLength(50)]
            public string Name { get; set; } = string.Empty;

            public Guid IdeaId { get; set; }
            public virtual Idea Idea { get; set; } = null!;
        }
    }
 
