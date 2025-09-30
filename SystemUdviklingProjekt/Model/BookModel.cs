namespace SystemUdviklingProjekt.Model
{
    public class BookModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public string? CreatedBy { get; set; }   // hvem oprettede bogen
        public string? RentedBy { get; set; }    // hvem har lejet bogen (null = ledig)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsAvailable => string.IsNullOrEmpty(RentedBy);
    }
}
