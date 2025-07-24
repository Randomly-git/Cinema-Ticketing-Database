using System;
using System.Security.Cryptography;
using System.Text;

namespace test.Helpers
{
    /// 密码哈希和验证的辅助类。
    /// 采用 PBKDF2 算法，增强密码安全性。
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 盐值大小（字节）
        private const int HashSize = 20; // 哈希值大小（字节）
        private const int Iterations = 10000; // 迭代次数

        /// 生成随机盐值
        /// returnBase64 编码的盐值字符串。
        public static string GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        /// 对密码进行哈希处理。
        /// <param name="password">明文密码。
        /// <param name="salt">盐值。
        /// 返回Base64 编码的哈希密码字符串。
        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                return Convert.ToBase64String(hash);
            }
        }

        /// 验证密码是否匹配存储的哈希值。
        /// <param name="password">用户输入的明文密码。
        /// <param name="storedHash">数据库中存储的哈希密码。
        /// <param name="storedSalt">数据库中存储的盐值。
        /// 如果密码匹配，则为 true；否则为 false。
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string newHash = HashPassword(password, storedSalt);
            return newHash == storedHash;
        }
    }
}
