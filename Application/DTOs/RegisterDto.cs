using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "نام کامل الزامی است")]
        [MaxLength(100, ErrorMessage = "نام کامل نمی‌تواند بیش از 100 کاراکتر باشد")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MinLength(6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Bio { get; set; }

        public string? Role { get; set; } // "Ideator", "Developer", "Investor"
    }

    //public class LoginDto
    //{
    //    [Required(ErrorMessage = "ایمیل الزامی است")]
    //    [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
    //    public string Email { get; set; } = string.Empty;

    //    [Required(ErrorMessage = "رمز عبور الزامی است")]
    //    public string Password { get; set; } = string.Empty;

    //    public bool RememberMe { get; set; }
    //}

    //public class AuthResponseDto
    //{
    //    public string Token { get; set; } = string.Empty;
    //    public string RefreshToken { get; set; } = string.Empty;
    //    public UserDto User { get; set; } = new();
    //    public DateTime ExpiresAt { get; set; }
    //}

    //public class UserDto
    //{
    //    public Guid Id { get; set; }
    //    public string FullName { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty;
    //    public string? Bio { get; set; }
    //    public string? AvatarUrl { get; set; }
    //    public string Role { get; set; } = string.Empty;
    //    public DateTime CreatedAt { get; set; }
    //    public DateTime? LastLoginAt { get; set; }

    //    // آمار کاربر
    //    public int TotalIdeas { get; set; }
    //    public int TotalReviews { get; set; }
    //    public int TotalCollaborations { get; set; }
    //}

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
        [MinLength(6, ErrorMessage = "رمز عبور جدید باید حداقل 6 کاراکتر باشد")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن مطابقت ندارند")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
        [MinLength(6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن مطابقت ندارند")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
 
