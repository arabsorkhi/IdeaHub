using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
   
        public interface IEncryptionService
        {
            /// <summary>
            /// رمزگذاری متن ساده
            /// </summary>
            Task<string> EncryptAsync(string plainText);

            /// <summary>
            /// رمزگشایی متن رمزگذاری شده
            /// </summary>
            Task<string> DecryptAsync(string cipherText);

            /// <summary>
            /// تولید کلید رمزگذاری جدید
            /// </summary>
            string GenerateKey();

            /// <summary>
            /// تولید IV جدید
            /// </summary>
            string GenerateIV();

            /// <summary>
            /// هش کردن رمز عبور
            /// </summary>
            string HashPassword(string password);

            /// <summary>
            /// اعتبارسنجی رمز عبور
            /// </summary>
            bool VerifyPassword(string password, string hash);
        }
     
}
