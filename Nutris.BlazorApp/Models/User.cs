namespace Nutris.BlazorApp.Models
{
    public class User
    {
        public string Name { get; set; }
        public string user { get; set; }
        public string Customer { get; set; }
        public string Logo { get; set; }
        public string Email { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Name { get; set; }
        public string Customer { get; set; }
        public string User { get; set; }
        public string Logo { get; set; }
        public string Token { get; set; }
    }
}