using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
namespace Application.Services
{
   
        public class EncryptionService : IEncryptionService
        {
            private readonly byte[] _key; // ذخیره شده در Azure Key Vault یا User Secret
            private readonly byte[] _iv;

            public EncryptionService(IConfiguration configuration)
            {
                var keyString = configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
                var ivString = configuration["Encryption:IV"] ?? throw new InvalidOperationException("Encryption IV not configured");

            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
            {
                // اگر کلید تنظیم نشده بود، کلید جدید تولید کن
                _key = GenerateKeyBytes();
                _iv = GenerateIVBytes();
            }
            else
            {
                _key = Convert.FromBase64String(keyString);
                _iv = Convert.FromBase64String(ivString);
            }
        }

            public async Task<string> EncryptAsync(string plainText)
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                await cs.WriteAsync(plainBytes, 0, plainBytes.Length);
                await cs.FlushFinalBlockAsync();

                return Convert.ToBase64String(ms.ToArray());
            }

            public async Task<string> DecryptAsync(string cipherText)
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

                var plainBytes = new byte[ms.Length];
                var bytesRead = await cs.ReadAsync(plainBytes, 0, plainBytes.Length);

                return Encoding.UTF8.GetString(plainBytes, 0, bytesRead);
            }
            public string GenerateKey()
            {
                return Convert.ToBase64String(GenerateKeyBytes());
            }

            public string GenerateIV()
            {
                return Convert.ToBase64String(GenerateIVBytes());
            }

            private byte[] GenerateKeyBytes()
            {
                using var aes = Aes.Create();
                aes.GenerateKey();
                return aes.Key;
            }

            private byte[] GenerateIVBytes()
            {
                using var aes = Aes.Create();
                aes.GenerateIV();
                return aes.IV;
            }

            public string HashPassword(string password)
            {
                using var sha256 = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }

            public bool VerifyPassword(string password, string hash)
            {
                return HashPassword(password) == hash;
            }
        }
}

       
 
