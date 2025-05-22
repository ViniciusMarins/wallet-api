using Bcrypt = BCrypt.Net.BCrypt;

namespace api.Utils
{
    public static class HashUtils
    {
        public static bool VerifyPassword(string textPassword, string hashPassword)
        {
            return Bcrypt.Verify(textPassword, hashPassword);
        }

        public static string HashPassword(string textPassword)
        {
            return Bcrypt.HashPassword(textPassword);
        }
    }
}
