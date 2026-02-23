namespace CrossCutting.Crypto
{
    public interface ICrypto
    {
        string Decrypt(string encryptedText);
        string Encrypt(string text);
    }
}
