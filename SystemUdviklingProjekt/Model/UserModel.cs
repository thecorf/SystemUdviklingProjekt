namespace SystemUdviklingProjekt.Model
{
    public class UserModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int ZipCode { get; set; } = int.MaxValue;
    }
}
