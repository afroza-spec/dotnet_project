namespace MyWebAPI.Models
{
    public class RegisterModel
    {
        public string? Name { get; set; } // User's full name

        public string? Email { get; set; } // User email

        public string? Password { get; set; } // User password

        public string? ConfirmPassword { get; set; } // Password confirmation

        public string? Role { get; set; } // Role: "Admin" or "Player"
    }
}
