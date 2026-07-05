using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{ 
        public class CreateIdeaDto
        {
            [Required]
            [MaxLength(200)]
            public string Title { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;

            public string? ProblemStatement { get; set; }
            public string? SolutionDescription { get; set; }
            public string? TargetAudience { get; set; }
            public string? UniqueValue { get; set; }

            public decimal? EstimatedBudget { get; set; }
            public int? EstimatedTimelineMonths { get; set; }

            public string? EncryptedDetails { get; set; }
            public string? EncryptionKeyHint { get; set; }

            public string? Visibility { get; set; } // "Private", "Confidential", "Public"
            public List<string>? Tags { get; set; }
        }

        public class UpdateIdeaDto
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? ProblemStatement { get; set; }
            public string? SolutionDescription { get; set; }
            public string? TargetAudience { get; set; }
            public string? UniqueValue { get; set; }
            public decimal? EstimatedBudget { get; set; }
            public int? EstimatedTimelineMonths { get; set; }
            public string? EncryptedDetails { get; set; }
            public string? EncryptionKeyHint { get; set; }
            public string? Visibility { get; set; }
            public List<string>? Tags { get; set; }
        }

        public class IdeaResponseDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string? ProblemStatement { get; set; }
            public string? SolutionDescription { get; set; }
            public string? TargetAudience { get; set; }
            public string? UniqueValue { get; set; }
            public decimal? EstimatedBudget { get; set; }
            public int? EstimatedTimelineMonths { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Visibility { get; set; } = string.Empty;
            public int ViewCount { get; set; }
            public int LikeCount { get; set; }
            public int ReviewCount { get; set; }
            public UserDto Owner { get; set; } = new();
            public DateTime CreatedAt { get; set; }
            public DateTime? PublishedAt { get; set; }
            public List<string> Tags { get; set; } = new();
            public bool IsOwner { get; set; }
            public bool CanEdit { get; set; }
        }

        public class IdeaListResponseDto
        {
            public List<IdeaResponseDto> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public bool HasNextPage { get; set; }
            public bool HasPreviousPage { get; set; }
        }

        public class IdeaReviewDto
        {
            [Required]
            [MaxLength(500)]
            public string Comment { get; set; } = string.Empty;

            public int? Rating { get; set; } // 1-5
            public string? Type { get; set; } // "General", "Technical", "Business", "Investment"
        }

        public class IdeaReviewResponseDto
        {
            public Guid Id { get; set; }
            public string Comment { get; set; } = string.Empty;
            public int? Rating { get; set; }
            public string Type { get; set; } = string.Empty;
            public UserDto Reviewer { get; set; } = new();
            public DateTime CreatedAt { get; set; }
        }

        public class IdeaCollaboratorDtoI
        {
            public Guid UserId { get; set; }
            public string Role { get; set; } = string.Empty; // "Developer", "Designer", etc.
        }
    }
 