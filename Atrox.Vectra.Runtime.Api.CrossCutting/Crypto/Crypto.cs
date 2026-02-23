namespace CrossCutting.Crypto
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using System.Security.Cryptography;
    using System.Text;

    public class Crypto : ICrypto
    {
        private readonly IConfiguration _conf;
        private readonly ILogger<Crypto> _logger;
        private readonly byte[] rgbIV;
        private readonly string key;
        public Crypto(IConfiguration config, ILogger<Crypto> logger)
        {
            _conf = config ?? throw new ArgumentNullException(nameof(config));
            rgbIV = Encoding.ASCII.GetBytes(_conf.GetSection("IV_CRYPT").Value);
            key = _conf.GetSection("KEY_CRYPT").Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public string Encrypt(string text)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = rgbIV;
            ICryptoTransform cryptTransform = cipher.CreateEncryptor();
            byte[] plaintext = Encoding.UTF8.GetBytes(text);
            byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);
            return Convert.ToBase64String(cipherText);
        }

        public string Decrypt(string encryptedText)
        {
            _logger.LogDebug("Decrypt operation started.");

            Aes cipher = CreateCipher(key);
            cipher.IV = rgbIV;
            ICryptoTransform cryptTransform = cipher.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            _logger.LogDebug("Decrypt operation completed.");
            return Encoding.UTF8.GetString(plainBytes);
        }

        private static Aes CreateCipher(string strkey)
        {
            Aes cipher = Aes.Create();
            cipher.Mode = CipherMode.CBC;
            cipher.Padding = PaddingMode.PKCS7;
            cipher.Key = Encoding.ASCII.GetBytes(strkey);
            return cipher;
        }
    }
}
