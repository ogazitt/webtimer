namespace WebTimer.Client.Models
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RegisterUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Name { get; set; }
    }
}
