using Application.DTOs;

namespace Domain.Interfaces
{
     
 
        public interface IAuthService
        {
            Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
            Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
            Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
            Task<bool> LogoutAsync(Guid userId);
            Task<UserDto> GetUserByIdAsync(Guid userId);
            Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
            Task<bool> ForgotPasswordAsync(string email);
            Task<bool> ResetPasswordAsync(string token, string newPassword);
            Task<bool> ValidateTokenAsync(string token);
        }
    }
 
