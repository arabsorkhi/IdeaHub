using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class IdeaCollaboratorDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty; // "Developer", "Designer", "Marketing", "Advisor", "Investor"

        public string? Message { get; set; } // پیام دعوت‌نامه
    }

    public class IdeaCollaboratorResponseDto
    {
        public Guid Id { get; set; }
        public Guid IdeaId { get; set; }
        public UserDto User { get; set; } = new();
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pending", "Active", "Inactive", "Rejected"
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
    }

    public class UpdateCollaboratorStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // "Active", "Inactive", "Rejected"
    }
}
