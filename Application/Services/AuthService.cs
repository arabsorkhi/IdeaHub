using Application.DTOs;
using Application.Interfaces;
 
using Domain.Entity;
using Domain.Interfaces;
 
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    
     
        public class AuthService : IAuthService
        {
          
            private readonly IRepository<User> _userRepository;
            private readonly IConfiguration _configuration;
            private readonly IEmailService _emailService;

            public AuthService(IRepository<User> userRepository, IConfiguration configuration, IEmailService emailService)
            {
                _userRepository = userRepository;
                _configuration = configuration;
                _emailService = emailService;
            }

            public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
            {
                // بررسی وجود کاربر
                var existingUser = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                if (existingUser != null)
                    throw new InvalidOperationException("این ایمیل قبلاً ثبت نام کرده است.");

                // ایجاد کاربر جدید
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    Bio = registerDto.Bio,
                    CreatedAt = DateTime.UtcNow,
                    Role = UserRole.Ideator,
                    Status = UserStatus.Active
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // تولید Token
                return await GenerateAuthResponseAsync(user);
            }

            public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                    throw new UnauthorizedAccessException("ایمیل یا رمز عبور اشتباه است.");

                if (user.Status != UserStatus.Active)
                    throw new UnauthorizedAccessException("حساب کاربری شما غیرفعال است.");

                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                return await GenerateAuthResponseAsync(user);
            }

            public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
            {
                // پیاده‌سازی Refresh Token
                // ...
                throw new NotImplementedException();
            }

            public async Task<bool> LogoutAsync(Guid userId)
            {
                // پیاده‌سازی Logout
                return await Task.FromResult(true);
            }

            public async Task<UserDto> GetUserByIdAsync(Guid userId)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException("کاربر یافت نشد.");

                return MapToUserDto(user);
            }

            public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException("کاربر یافت نشد.");

                if (!VerifyPassword(currentPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("رمز عبور فعلی اشتباه است.");

                user.PasswordHash = HashPassword(newPassword);
                await _userRepository.SaveChangesAsync();
                return true;
            }

            public async Task<bool> ForgotPasswordAsync(string email)
            {
                var user = await _userRepository.Query()
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    var token = GeneratePasswordResetToken();
                    await _emailService.SendPasswordResetEmailAsync(email, token); //?
                }

                // همیشه True برگردانید تا حملات Brute Force محدود شود
                return true;
            }

            public async Task<bool> ResetPasswordAsync(string token, string newPassword)
            {
                // پیاده‌سازی Reset Password با توکن
                // ...
                return await Task.FromResult(true);
            }

            public async Task<bool> ValidateTokenAsync(string token)
            {
                // پیاده‌سازی اعتبارسنجی توکن
                return await Task.FromResult(true);
            }

            #region Private Methods

            private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user)
            {
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = MapToUserDto(user),
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }

            private string GenerateJwtToken(User user)
            {
                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FullName", user.FullName)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            private string GenerateRefreshToken()
            {
                var randomNumber = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }

            private string HashPassword(string password)
            {
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }

            private bool VerifyPassword(string password, string hash)
            {
                return HashPassword(password) == hash;
            }

            private string GeneratePasswordResetToken()
            {
                return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            }

            private UserDto MapToUserDto(User user)
            {
                return new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Bio = user.Bio,
                    AvatarUrl = user.AvatarUrl,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                };
            }

        #endregion





        // اضافه کردن این متدها به کلاس AuthService

        //public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        //{
        //    // اعتبارسنجی Refresh Token
        //    // در اینجا یک پیاده‌سازی ساده
        //    var user = await _userRepository.Query()
        //        .FirstOrDefaultAsync(u => u.Email == "test@example.com"); // TODO: ذخیره Refresh Token در دیتابیس

        //    if (user == null)
        //        throw new UnauthorizedAccessException("توکن نامعتبر است.");

        //    return await GenerateAuthResponseAsync(user);
        //}

        //public async Task<bool> LogoutAsync(Guid userId)
        //{
        //    // در اینجا می‌توانید توکن را در لیست سیاه قرار دهید
        //    return await Task.FromResult(true);
        //}

        //public async Task<bool> ValidateTokenAsync(string token)
        //{
        //    try
        //    {
        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException());

        //        tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = true,
        //            ValidIssuer = _configuration["Jwt:Issuer"],
        //            ValidateAudience = true,
        //            ValidAudience = _configuration["Jwt:Audience"],
        //            ValidateLifetime = true,
        //            ClockSkew = TimeSpan.Zero
        //        }, out _);

        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        //public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        //{
        //    // پیاده‌سازی با استفاده از توکن ذخیره شده در دیتابیس
        //    // اینجا یک پیاده‌سازی ساده
        //    return await Task.FromResult(true);
        //}
    }
    }
 
