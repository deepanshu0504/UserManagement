using System.Security.Cryptography;
using System.Text;

namespace LoginandRegisterMVC.Services;

public interface IPasswordHashService
{
    string HashPassword(string password);
}

public class PasswordHashService : IPasswordHashService
{
    public string HashPassword(string password)
    {
        var pwdarray = Encoding.ASCII.GetBytes(password);
        var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(pwdarray);
        var hashpwd = new StringBuilder(hash.Length);
        foreach (byte b in hash)
        {
            hashpwd.Append(b.ToString());
        }
        return hashpwd.ToString();
    }
}
