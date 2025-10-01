namespace SystemUdviklingProjekt.Model
{
    public class UserModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";  
        public string Email { get; set; } = "";
        public string? Description { get; set; }
        public int? ZipCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
