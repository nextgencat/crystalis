using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;

namespace CrystalisAPI
{
    internal class Encryptor
    {
        private const int KeySize = 256;
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int Iterations = 100000;

        public static string Encrypt(string plaintext, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(plaintext))
                    throw new ArgumentException("Plaintext cannot be empty");
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("Password cannot be empty");
                byte[] randomBytes1 = GenerateRandomBytes(16);
                byte[] randomBytes2 = GenerateRandomBytes(12);
                byte[] numArray = DeriveKey(password, randomBytes1);
                byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] src1 = new byte[bytes.Length];
                byte[] src2 = new byte[16];
                using (AesGcm aesGcm = new AesGcm(numArray))
                    aesGcm.Encrypt(randomBytes2, bytes, src1, src2, (byte[])null);
                byte[] dst = new byte[randomBytes1.Length + randomBytes2.Length + src1.Length + src2.Length];
                Buffer.BlockCopy((Array)randomBytes1, 0, (Array)dst, 0, randomBytes1.Length);
                Buffer.BlockCopy((Array)randomBytes2, 0, (Array)dst, randomBytes1.Length, randomBytes2.Length);
                Buffer.BlockCopy((Array)src1, 0, (Array)dst, randomBytes1.Length + randomBytes2.Length, src1.Length);
                Buffer.BlockCopy((Array)src2, 0, (Array)dst, randomBytes1.Length + randomBytes2.Length + src1.Length, src2.Length);
                return Convert.ToBase64String(dst);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string Decrypt(string ciphertext, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(ciphertext))
                    throw new ArgumentException("Ciphertext cannot be empty");
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentException("Password cannot be empty");
                byte[] src;
                try
                {
                    src = Convert.FromBase64String(ciphertext);
                }
                catch (FormatException ex)
                {
                    throw;
                }
                int num = 44;
                if (src.Length < num)
                    throw new CryptographicException("Invalid ciphertext: data too short");
                byte[] numArray1 = new byte[16];
                byte[] dst1 = new byte[12];
                int count = src.Length - 16 - 12 - 16;
                byte[] dst2 = new byte[count];
                byte[] dst3 = new byte[16];
                Buffer.BlockCopy((Array)src, 0, (Array)numArray1, 0, 16);
                Buffer.BlockCopy((Array)src, 16, (Array)dst1, 0, 12);
                Buffer.BlockCopy((Array)src, 28, (Array)dst2, 0, count);
                Buffer.BlockCopy((Array)src, 28 + count, (Array)dst3, 0, 16 );
                byte[] numArray2 = DeriveKey(password, numArray1);
                byte[] numArray3 = new byte[count];
                using (AesGcm aesGcm = new AesGcm(numArray2))
                    aesGcm.Decrypt(dst1, dst2, dst3, numArray3, (byte[])null);
                return Encoding.UTF8.GetString(numArray3);
            }
            catch (CryptographicException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
                return ((DeriveBytes)rfc2898DeriveBytes).GetBytes(32);
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
                randomNumberGenerator.GetBytes(randomBytes);
            return randomBytes;
        }
    }
}
