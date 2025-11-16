using System;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        var password = "Pa$$w0rd123!";
        byte[] salt;
        byte[] subkey;
        using (var deriveBytes = new Rfc2898DeriveBytes(password, 16, 1000))
        {
            salt = deriveBytes.Salt;
            subkey = deriveBytes.GetBytes(32);
        }
        var dst = new byte[49];
        Buffer.BlockCopy(salt, 0, dst, 1, 16);
        Buffer.BlockCopy(subkey, 0, dst, 17, 32);
        var hash = Convert.ToBase64String(dst);
        Console.WriteLine(hash);
    }
}