using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{ 
        public class AuthDtos
        {
            [Required]
            [MaxLength(100)]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            public string Password { get; set; } = string.Empty;

            public string? Bio { get; set; }
        }

        public class LoginDto
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public class AuthResponseDto
        {
            public string Token { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public UserDto User { get; set; } = new();
            public DateTime ExpiresAt { get; set; }
        }

        public class UserDto
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Bio { get; set; }
            public string? AvatarUrl { get; set; }
            public string Role { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
 
