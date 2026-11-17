using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OrderManagement.Application.Common.Hasher
{
    public static class RequestHasher
    {
        public static string Compute<T>(T request)
        {
            ArgumentNullException.ThrowIfNull(request);

            string json = JsonSerializer.Serialize(request);

            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));

            return Convert.ToHexString(hashBytes);
        }
    }
}
