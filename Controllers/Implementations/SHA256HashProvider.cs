using System.Security.Cryptography;

namespace Stratis.MediaConverterApi
{
    public class SHA256HashProvider : IHashProvider
    {
        public string GetFileHash(string fileName)
        {
            using (var SHA256Instance = SHA256.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var hash = SHA256Instance.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}