using BCrypt.Net;

namespace RecruitmentAPI.Helpers
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// Utility for hashing and verifying passwords using BCrypt.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Hashes a plain-text password.
        /// </summary>
        public string HashPassword(string password)
        {
            // The default work factor (11) is generally safe, but can be adjusted if needed
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plain-text password against a hashed password.
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}