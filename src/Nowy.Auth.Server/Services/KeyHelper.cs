using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Nowy.Auth.Server.Services;

public static class KeyHelper
{
    private static SymmetricSecurityKey? _cache_symmetric_key;

    public static SymmetricSecurityKey GetSymmetricKey()
    {
        if (_cache_symmetric_key is { }) return _cache_symmetric_key;

        List<string> paths = new()
        {
            "/opt/nowy/auth/symmetric-key.txt",
            "/opt/ts/auth/symmetric-key.txt",
        };

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(content))
                {
                    return _cache_symmetric_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(content));
                }
            }
        }

        throw new InvalidOperationException($"Symmetric Key not found!");
    }
}
