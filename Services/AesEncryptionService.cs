using System.Security.Cryptography;
using System.Text;

namespace MVPDS.Services
{
    public static class AesEncryptionService
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("a8$Dk!f7Ld#93jVb");

        public static string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var encrypted = ms.ToArray();
            var result = iv.Concat(encrypted).ToArray();

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string cipherText)
        {
            var fullBytes = Convert.FromBase64String(cipherText);

            if (fullBytes.Length < 16)
                throw new ArgumentException("Cipher text is too short.");

            var iv = fullBytes.Take(16).ToArray();
            var cipher = fullBytes.Skip(16).ToArray();

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}